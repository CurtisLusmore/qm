using be.Shared;

namespace be.HostedServices;

public partial class DownloadManagementService
{
    private async Task DownloadTorrentFilesAsync(CancellationToken cancellationToken)
    {
        foreach (var tracker in trackers.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await DownloadTorrentFileAsync(tracker, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Unexpected error when downloading torrent file for {InfoHash}: {Reason}", tracker.InfoHash, ex.Message);
            }
        }
    }

    private async Task DownloadTorrentFileAsync(DownloadTracker tracker, CancellationToken cancellationToken)
    {
        if (tracker.Status != DownloadStatus.Received) return;
        var infoHash = tracker.InfoHash.ToUpperInvariant();
        trackers[infoHash] = tracker with { Status = DownloadStatus.DownloadingTorrentFile };
        var targetFilename = TorrentFile(infoHash);
        if (!File.Exists(targetFilename))
        {
            var tmpFilename = $"{targetFilename}.tmp";
            logger.LogInformation("Downloading torrent file for {InfoHash}: {Name}", infoHash, tracker.Title.Name);
            using var httpClient = httpClientFactory.CreateClient();
            var url = $"https://itorrents.net/torrent/{infoHash}.torrent";
            try
            {
                using var readStream = await httpClient.GetStreamAsync(url, cancellationToken);
                using var writeStream = File.OpenWrite(tmpFilename);
                await readStream.CopyToAsync(writeStream, cancellationToken);
                File.Move(tmpFilename, targetFilename, true);
                logger.LogInformation("Downloaded torrent file for {InfoHash}: {Name}", infoHash, tracker.Title.Name);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Failed to download torrent file for {InfoHash}: {Reason}", infoHash, ex.Message);
                trackers[infoHash] = tracker with { Status = DownloadStatus.DownloadTorrentFileFailed };
                return;
            }
        }
        else
        {
            logger.LogInformation("Using existing torrent file for {InfoHash}: {Name}", infoHash, tracker.Title.Name);
        }
        trackers[infoHash] = tracker with { Status = DownloadStatus.DownloadedTorrentFile };
    }
}
