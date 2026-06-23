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
            try
            {
                await AddTorrent(tracker, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error when adding torrent for {InfoHash}: {Reason}", tracker.InfoHash, ex.Message);
            }
        }
    }

    private async Task AddTorrent(DownloadTracker tracker, CancellationToken cancellationToken)
    {
        if (tracker.Status != DownloadStatus.DownloadedTorrentFile) return;
        TorrentManager manager;
        try
        {
            var torrent = Torrent.Load(Path.Combine(rootDirectory, "torrents", $"{tracker.InfoHash.ToUpperInvariant()}.torrent"));
            manager = await torrentEngine.AddAsync(torrent, Path.Combine(rootDirectory, "torrents"));
        }
        catch (TorrentException ex)
        {
            logger.LogWarning(ex, "Invalid torrent file for {InfoHash}: {Reason}. Trying magnet link...", tracker.InfoHash, ex.Message);
            File.Delete(Path.Combine(rootDirectory, "torrents", $"{tracker.InfoHash.ToUpperInvariant()}.torrent"));
            var magnetLink = MagnetLink.FromUri(new Uri($"magnet:?xt=urn:btih:{tracker.InfoHash}&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969%2Fannounce&tr=udp%3A%2F%2Ftracker.opentrackr.org%3A1337%2Fannounce&tr=udp%3A%2F%2Fopen.stealth.si%3A80%2Fannounce"));
            manager = await torrentEngine.AddAsync(magnetLink, Path.Combine(rootDirectory, "torrents"));
            // logger.LogError(ex, "Failed to add torrent for {InfoHash}: {Reason}", tracker.InfoHash, ex.Message);
            // trackers[tracker.InfoHash] = tracker with {
            //     Status = DownloadStatus.DownloadTorrentFailed,
            //     Error = "Invalid torrent file. Try removing this torrent and downloading it again",
            // };
            // return;
        }
        logger.LogInformation("Added torrent for {InfoHash}: {Name}", tracker.InfoHash, tracker.Title.Name);
        managers[tracker.InfoHash] = manager;
        trackers[tracker.InfoHash] = tracker with { Status = DownloadStatus.AddedTorrent };
        if (FastResume.TryLoad(Path.Combine(rootDirectory, "torrents", $"{tracker.InfoHash.ToUpperInvariant()}.fastresume"), out var fastResume))
        {
            await manager.LoadFastResumeAsync(fastResume);
            logger.LogInformation("Loaded fast resume for {InfoHash}: {Name}", tracker.InfoHash, tracker.Title.Name);
        }
        await manager.StartAsync();
        trackers[tracker.InfoHash] = tracker with { Status = DownloadStatus.StartedTorrent };
        logger.LogInformation("Started torrent for {InfoHash}: {Name}", tracker.InfoHash, tracker.Title.Name);
    }
}
