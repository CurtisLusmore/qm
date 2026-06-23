using be.Shared;
using FifteenthStandard.Storage;
using MonoTorrent;
using MonoTorrent.Client;

namespace be.HostedServices;

public partial class DownloadManagementService(
    IHttpClientFactory httpClientFactory,
    IKeyValueStore keyValueStore,
    string rootDirectory,
    ILogger<DownloadManagementService> logger) : BackgroundService
{
    private readonly IDictionary<string, DownloadTracker> trackers = new Dictionary<string, DownloadTracker>();
    private readonly IDictionary<string, TorrentManager> managers = new Dictionary<string, TorrentManager>();
    private readonly ClientEngine torrentEngine = new ClientEngine(
        new EngineSettingsBuilder
        {
            CacheDirectory = Path.Combine(rootDirectory, "torrents", "cache"),
        }.ToSettings());

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Run(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Cancellation requested.");
        }
        finally
        {
            await CleanupAsync();
        }
    }

    private async Task Run(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Running DownloadManagementService...");
            await DetectSavedDownloadsAsync(cancellationToken);
            await DownloadTorrentFilesAsync(cancellationToken);
            await AddTorrents(cancellationToken);
            await WatchTorrents(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Caught elsewhere for cleanup
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in DownloadManagementService: {Reason}", ex.Message);
        }
    }

    private async Task CleanupAsync()
    {
        foreach (var (infoHash, manager) in managers)
        {
            try
            {
                await manager.StopAsync();
                await manager.SaveFastResumeAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error stopping torrent manager for {InfoHash}: {Reason}", infoHash, ex.Message);
            }
        }

        await torrentEngine.StopAllAsync();
        torrentEngine.Dispose();
        logger.LogInformation("DownloadManagementService has been stopped and cleaned up.");
    }
}
