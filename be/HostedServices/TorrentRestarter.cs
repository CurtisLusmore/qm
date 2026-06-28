using MonoTorrent.Client;

namespace be.HostedServices;

public partial class DownloadManagementService
{
    private async Task RestartFailedTorrentsAsync(CancellationToken cancellationToken)
    {
        foreach (var (infoHash, manager) in managers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await RestartFailedTorrentAsync(infoHash, manager, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Unexpected error when restarting torrent for {InfoHash}: {Reason}", infoHash, ex.Message);
            }
        }
    }

    private async Task RestartFailedTorrentAsync(string infoHash, TorrentManager manager, CancellationToken cancellationToken)
    {
        if (manager.State != TorrentState.Error) return;
        await manager.StopAsync();
        await manager.StartAsync();
        logger.LogInformation("Restarted failed torrent for {InfoHash}...", infoHash);
    }
}
