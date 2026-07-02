using be.Interfaces;
using be.Shared;
using MonoTorrent;
using MonoTorrent.Client;

namespace be.HostedServices;

public partial class DownloadManagementService : IDownloadRemover
{
    public void RequestRemoveDownload(string infoHash)
    {
        try
        {
            trackers[infoHash] = trackers[infoHash] with { Status = DownloadStatus.Removing };
            logger.LogInformation("Requested removal of download for {InfoHash}", infoHash);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to remove download for {InfoHash}: {Reason}", infoHash, ex.Message);
            throw;
        }
    }

    private async Task RemoveDownloadsAsync(CancellationToken cancellationToken)
    {
        foreach (var (infoHash, tracker) in trackers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await RemoveDownloadAsync(infoHash, tracker, cancellationToken);
        }
    }

    private async Task RemoveDownloadAsync(string infoHash, DownloadTracker tracker, CancellationToken cancellationToken)
    {
        if (tracker.Status != DownloadStatus.Removing) return;
        var manager = managers[infoHash];
        if (manager.State == TorrentState.Stopping) return;
        if (manager.State != TorrentState.Stopped)
        {
            manager.StopAsync(); // Deliberately no awaited
            logger.LogInformation("Stopping download for {InfoHash} before removal", infoHash);
            return;
        }
        await torrentEngine.RemoveAsync(manager);
        managers.Remove(infoHash);
        Directory.Delete(TitleDirectory(downloadingDirectory, tracker.Title, infoHash), true);
        logger.LogInformation("Deleted files for {InfoHash} from downloading directory", infoHash);
        SafeDelete(TorrentFile(infoHash));
        SafeDelete(TitleFile(infoHash));
        SafeDelete(FastresumeFile(infoHash));
        trackers.Remove(infoHash);
        logger.LogInformation("Removed download for {InfoHash}", infoHash);

        void SafeDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    logger.LogInformation("Deleted file {Path}", path);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete file {Path}: {Reason}", path, ex.Message);
            }
        }
    }
}
