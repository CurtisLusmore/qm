using be.Shared;
using MonoTorrent;
using MonoTorrent.Client;

namespace be.HostedServices;

public partial class DownloadManagementService
{
    private async Task WatchTorrents(CancellationToken cancellationToken)
    {
        foreach (var (infoHash, manager) in managers)
        {
            try
            {
                await WatchTorrent(infoHash, manager, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error when watching torrent for {InfoHash}: {Reason}", infoHash, ex.Message);
            }
        }
    }

    private async Task WatchTorrent(string infoHash, TorrentManager manager, CancellationToken cancellationToken)
    {
        var tracker = trackers[infoHash];
        tracker = UpdateProgress(tracker, manager) with {
            Seeds = manager.Peers.Seeds
        };

        switch (manager.State)
        {
            case TorrentState.Metadata:
            case TorrentState.FetchingHashes:
            case TorrentState.Hashing:
            case TorrentState.Starting:
                logger.LogInformation("Torrent {InfoHash} is in state {State}", infoHash, manager.State);
                tracker = tracker with { Status = DownloadStatus.InitializingTorrent };
                break;
            case TorrentState.Downloading:
                tracker = tracker with { Status = DownloadStatus.DownloadingTorrent };
                break;
            case TorrentState.HashingPaused:
            case TorrentState.Paused:
                tracker = tracker with { Status = DownloadStatus.PausedTorrent };
                break;
            case TorrentState.Seeding:
            case TorrentState.Stopping:
                tracker = tracker with { Status = DownloadStatus.StoppingTorrent };
                break;
            case TorrentState.Stopped:
                tracker = tracker with { Status = DownloadStatus.StoppedTorrent };
                break;

            case TorrentState.Error:
                tracker = tracker with { Status = DownloadStatus.DownloadTorrentFailed };
                break;
        }

        trackers[infoHash] = tracker;
        logger.LogInformation(
            "Torrent {InfoHash} is {Status} at {PartialProgress}% (target: {TargetProgress}%, total: {TotalProgress}%) at {BytesPerSecond}B/s with {Seeds} seeds",
            infoHash,
            tracker.Status,
            tracker.PartialProgressPercent,
            tracker.TargetProgressPercent,
            tracker.TotalProgressPercent,
            tracker.BytesPerSecond,
            tracker.Seeds);
    }

    private static DownloadTracker UpdateProgress(DownloadTracker tracker, TorrentManager manager)
    {
        var files = manager.Files;
        var targetFiles = files.Where(f => f.Priority != Priority.DoNotDownload).ToList();
        var downloadedBytes = targetFiles.Sum(f => f.BytesDownloaded());
        var targetBytes = targetFiles.Sum(f => f.Length);
        var totalBytes = files.Sum(f => f.Length);

        var partialProgressPercent = targetBytes == 0 ? 0 : 100.0 * downloadedBytes / targetBytes;
        var targetProgressPercent = targetBytes == 0 ? 0 : 100.0 * targetBytes / totalBytes;
        var totalProgressPercent = totalBytes == 0 ? 0 : 100.0 * downloadedBytes / totalBytes;

        return tracker with {
            DownloadedBytes = downloadedBytes,
            TargetBytes = targetBytes,
            TotalBytes = totalBytes,
            PartialProgressPercent = partialProgressPercent,
            TargetProgressPercent = targetProgressPercent,
            TotalProgressPercent = totalProgressPercent,
            BytesPerSecond = manager.Monitor.DownloadRate,
        };
    }
}
