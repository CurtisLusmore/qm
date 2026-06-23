using be.Shared;

namespace be.HostedServices;

public partial class DownloadManagementService
{
    private async Task DownloadTorrentFilesAsync(CancellationToken stoppingToken)
    {
        foreach (var tracker in trackers.Values)
        {
            try
            {
                await DownloadTorrentFileAsync(tracker, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error when downloading torrent file for {InfoHash}: {Reason}", tracker.InfoHash, ex.Message);
            }
        }
    }

    private async Task DownloadTorrentFileAsync(DownloadTracker tracker, CancellationToken stoppingToken)
    {
        if (tracker.Status != DownloadStatus.Received) return;
        var infoHash = tracker.InfoHash.ToUpperInvariant();
        var targetFilename = Path.Combine(rootDirectory, "torrents", $"{infoHash}.torrent");
        if (!File.Exists(targetFilename))
        {
            var tmpFilename = $"{targetFilename}.tmp";
            logger.LogInformation("Downloading torrent file for {InfoHash}: {Name}", infoHash, tracker.Title.Name);
            using var httpClient = httpClientFactory.CreateClient();
            var url = $"https://itorrents.net/torrent/{infoHash}.torrent";
            try
            {
                using var readStream = await httpClient.GetStreamAsync(url, stoppingToken);
                using var writeStream = File.OpenWrite(tmpFilename);
                await readStream.CopyToAsync(writeStream, stoppingToken);
                File.Move(tmpFilename, targetFilename, true);
                logger.LogInformation("Downloaded torrent file for {InfoHash}: {Name}", infoHash, tracker.Title.Name);
            }
            catch (Exception ex)
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
