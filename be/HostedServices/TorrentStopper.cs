using be.Shared;
using MonoTorrent;
using MonoTorrent.Client;

namespace be.HostedServices;

public partial class DownloadManagementService
{
    private async Task StopTorrents(CancellationToken cancellationToken)
    {
        foreach (var (infoHash, manager) in managers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await StopCompletedTorrent(infoHash, manager, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Unexpected error when stopping torrent for {InfoHash}: {Reason}", infoHash, ex.Message);
            }
        }
    }

    private async Task StopCompletedTorrent(string infoHash, TorrentManager manager, CancellationToken cancellationToken)
    {
        if (manager.State != TorrentState.Seeding) return;

        logger.LogInformation("Stopping torrent {InfoHash} in state {State}", infoHash, manager.State);
        manager.StopAsync(); // Deliberately no awaited
    }
}
