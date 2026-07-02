using MonoTorrent.Client;

namespace be.HostedServices;

public partial class DownloadManagementService
{
    private async Task StopTorrentsAsync(CancellationToken cancellationToken)
    {
        foreach (var (infoHash, manager) in managers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await StopCompletedTorrentAsync(infoHash, manager, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Unexpected error when stopping torrent for {InfoHash}: {Reason}", infoHash, ex.Message);
            }
        }
    }

    private async Task StopCompletedTorrentAsync(string infoHash, TorrentManager manager, CancellationToken cancellationToken)
    {
        if (manager.State != TorrentState.Seeding) return;

        logger.LogInformation("Stopping torrent {InfoHash} in state {State}", infoHash, manager.State);
        manager.StopAsync(); // Deliberately no awaited
    }
}
