using be.Shared;
using MonoTorrent;
using MonoTorrent.Client;

namespace be.HostedServices;

public partial class DownloadManagementService
{
    private async Task FinalizeTorrentsAsync(CancellationToken cancellationToken)
    {
        foreach (var (infoHash, manager) in managers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await FinalizeTorrentAsync(infoHash, manager, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Unexpected error when finalizing torrent for {InfoHash}: {Reason}", infoHash, ex.Message);
            }
        }
    }

    private async Task FinalizeTorrentAsync(string infoHash, TorrentManager manager, CancellationToken cancellationToken)
    {
        if (manager.State != TorrentState.Stopped) return;

        var title = trackers[infoHash].Title;
        MoveFiles();
        DeleteFiles();
        DeleteFastResumeFile();
        DeleteTorrentFile();
        await torrentEngine.RemoveAsync(manager);
        managers.Remove(infoHash);

        trackers[infoHash] = trackers[infoHash] with { Status = DownloadStatus.DownloadedTorrent };
        logger.LogInformation("Finalized torrent {InfoHash}", infoHash);

        void MoveFiles()
        {
            var directory = TitleDirectory(completedDirectory, title, infoHash);
            Directory.CreateDirectory(directory);
            File.Move(TitleFile(infoHash), Path.Combine(directory, $"{infoHash}.json"), true);
            foreach (var file in manager.Files)
            {
                if (file.Priority == Priority.DoNotDownload) continue;
                try
                {
                    var finalPath = Path.Combine(directory, file.Path);
                    var finalDir = Path.GetDirectoryName(finalPath)!;
                    if (!Directory.Exists(finalDir))
                    {
                        Directory.CreateDirectory(finalDir);
                    }
                    File.Move(file.FullPath, finalPath);
                    logger.LogInformation("Moved file {FilePath} for torrent {InfoHash} to {FinalPath}", file.FullPath, infoHash, finalPath);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error moving file {FilePath} for torrent {InfoHash}: {Reason}", file.FullPath, infoHash, ex.Message);
                }
            }
        }

        void DeleteFiles()
        {
            var directory = TitleDirectory(downloadingDirectory, title, infoHash);
            if (!Directory.Exists(directory)) return;
            Directory.Delete(directory, true);
            logger.LogInformation("Deleted torrent directory for {InfoHash}", infoHash);
        }

        void DeleteTorrentFile()
        {
            var torrentFile = TorrentFile(infoHash);
            if (File.Exists(torrentFile))
            {
                File.Delete(torrentFile);
                logger.LogInformation("Deleted torrent file for {InfoHash}", infoHash);
            }
        }

        void DeleteFastResumeFile()
        {
            var fastResumeFile = FastresumeFile(infoHash);
            if (File.Exists(fastResumeFile))
            {
                File.Delete(fastResumeFile);
                logger.LogInformation("Deleted fast resume file for {InfoHash}", infoHash);
            }
        }
    }
}
