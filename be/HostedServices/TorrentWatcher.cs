using be.Shared;
using MonoTorrent;
using MonoTorrent.Client;

namespace be.HostedServices;

public partial class DownloadManagementService
{
    private async Task WatchTorrentsAsync(CancellationToken cancellationToken)
    {
        foreach (var (infoHash, manager) in managers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await WatchTorrentAsync(infoHash, manager, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Unexpected error when watching torrent for {InfoHash}: {Reason}", infoHash, ex.Message);
            }
        }
    }

    private async Task WatchTorrentAsync(string infoHash, TorrentManager manager, CancellationToken cancellationToken)
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
                tracker = tracker with { Status = DownloadStatus.DownloadedTorrent };
                break;

            case TorrentState.Error:
                tracker = tracker with { Status = DownloadStatus.DownloadTorrentFailed };
                break;
        }

        trackers[infoHash] = tracker;
        logger.LogDebug(
            "Torrent {InfoHash} is {Status} at {PartialProgress:0.0}% (target: {TargetProgress:0.0}%, total: {TotalProgress:0.0}%) at {BytesPerSecond}B/s with {Seeds} seeds",
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
        var files = manager.Files.Select(UpdateProgress).ToArray();
        var targetFiles = files.Where(f => f.Priority != FilePriority.Skip).ToList();
        var downloadedBytes = targetFiles.Sum(f => f.DownloadedBytes);
        var targetBytes = targetFiles.Sum(f => f.TotalBytes);
        var totalBytes = files.Sum(f => f.TotalBytes);

        var partialProgressPercent = targetBytes == 0 ? 0 : 100.0 * downloadedBytes / targetBytes;
        var targetProgressPercent = targetBytes == 0 ? 0 : 100.0 * targetBytes / totalBytes;
        var totalProgressPercent = totalBytes == 0 ? 0 : 100.0 * downloadedBytes / totalBytes;

        return tracker with {
            Files = files,
            DownloadedBytes = downloadedBytes,
            TargetBytes = targetBytes,
            TotalBytes = totalBytes,
            PartialProgressPercent = partialProgressPercent,
            TargetProgressPercent = targetProgressPercent,
            TotalProgressPercent = totalProgressPercent,
            BytesPerSecond = manager.Monitor.DownloadRate,
        };
    }

    private static FileTracker UpdateProgress(ITorrentManagerFile file)
    {
        return new FileTracker(
            file.Path,
            file.Priority switch
            {
                Priority.DoNotDownload => FilePriority.Skip,
                Priority.Lowest => FilePriority.Low,
                Priority.Low => FilePriority.Low,
                Priority.Normal => FilePriority.Normal,
                Priority.High => FilePriority.High,
                Priority.Highest => FilePriority.High,
                Priority.Immediate => FilePriority.High,
                _ => FilePriority.Normal,
            },
            file.BytesDownloaded(),
            file.Length,
            file.Length == 0 ? 0 : 100.0 * file.BytesDownloaded() / file.Length);
    }
}
