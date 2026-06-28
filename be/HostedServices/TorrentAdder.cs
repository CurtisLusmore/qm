using be.Shared;
using MonoTorrent;
using MonoTorrent.Client;

namespace be.HostedServices;

public partial class DownloadManagementService
{
    private async Task AddTorrents(CancellationToken cancellationToken)
    {
        foreach (var tracker in trackers.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await AddTorrent(tracker, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Unexpected error when adding torrent for {InfoHash}: {Reason}", tracker.InfoHash, ex.Message);
            }
        }
    }

    private async Task AddTorrent(DownloadTracker tracker, CancellationToken cancellationToken)
    {
        if (tracker.Status != DownloadStatus.DownloadedTorrentFile) return;

        TorrentManager manager;
        var directory = TitleDirectory(downloadingDirectory, tracker.Title, tracker.InfoHash);
        var settings = new TorrentSettingsBuilder { CreateContainingDirectory = false }.ToSettings();
        try
        {
            var torrent = Torrent.Load(TorrentFile(tracker.InfoHash));
            manager = await torrentEngine.AddAsync(torrent, directory, settings);
        }
        catch (TorrentException ex)
        {
            logger.LogWarning(ex, "Invalid torrent file for {InfoHash}: {Reason}. Trying magnet link...", tracker.InfoHash, ex.Message);
            File.Delete(TorrentFile(tracker.InfoHash));
            var magnetLink = MagnetLink.FromUri(new Uri($"magnet:?xt=urn:btih:{tracker.InfoHash}&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969%2Fannounce&tr=udp%3A%2F%2Ftracker.opentrackr.org%3A1337%2Fannounce&tr=udp%3A%2F%2Fopen.stealth.si%3A80%2Fannounce"));
            manager = await torrentEngine.AddAsync(magnetLink, directory, settings);
        }

        logger.LogInformation("Added torrent for {InfoHash}: {Name}", tracker.InfoHash, tracker.Title.Name);
        managers[tracker.InfoHash] = manager;
        trackers[tracker.InfoHash] = tracker with { Status = DownloadStatus.AddedTorrent };

        if (FastResume.TryLoad(FastresumeFile(tracker.InfoHash), out var fastResume))
        {
            await manager.LoadFastResumeAsync(fastResume);
            logger.LogInformation("Loaded fast resume for {InfoHash}: {Name}", tracker.InfoHash, tracker.Title.Name);
        }

        await manager.StartAsync();

        trackers[tracker.InfoHash] = tracker with { Status = DownloadStatus.StartedTorrent };
        logger.LogInformation("Started torrent for {InfoHash}: {Name}", tracker.InfoHash, tracker.Title.Name);
    }
}
