using be.Shared;
using Microsoft.Extensions.Options;
using MonoTorrent.Client;

namespace be.HostedServices;

public partial class DownloadManagementService(
    IHttpClientFactory httpClientFactory,
    IOptions<DownloadManagementService.Config> config,
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<DownloadManagementService> logger) : BackgroundService
{
    private readonly string titlesDirectory = Path.Combine(config.Value.RootDirectory, ".torrents", "titles");
    private readonly string metadataDirectory = Path.Combine(config.Value.RootDirectory, ".torrents", "metadata");
    private readonly string fastresumeDirectory = Path.Combine(config.Value.RootDirectory, ".torrents", "fastresume");
    private readonly string downloadingDirectory = Path.Combine(config.Value.RootDirectory, ".torrents", "downloading");
    private readonly string completedDirectory = Path.Combine(config.Value.RootDirectory, ".torrents", "completed");
    private readonly string binDirectory = Path.Combine(config.Value.RootDirectory, ".torrents", "bin");

    private readonly string moviesDirectory = Path.Combine(config.Value.RootDirectory, "Movies");
    private readonly string moviesTitlesDirectory = Path.Combine(config.Value.RootDirectory, "Movies", ".titles");
    private readonly string seriesDirectory = Path.Combine(config.Value.RootDirectory, "Series");
    private readonly string seriesTitlesDirectory = Path.Combine(config.Value.RootDirectory, "Series", ".titles");

    private string TitleFile(string infoHash) => Path.Combine(titlesDirectory, $"{infoHash.ToUpperInvariant()}.json");
    private string TorrentFile(string infoHash) => Path.Combine(metadataDirectory, $"{infoHash.ToUpperInvariant()}.torrent");
    private string FastresumeFile(string infoHash) => Path.Combine(fastresumeDirectory, $"{infoHash.ToUpperInvariant()}.fresume");
    private string TitleDirectory(string root, MovieOrSeries title, string? infoHash = null) => Path.Combine(root, $"{title.Name} ({title.Year}) [{title.Id}]{(infoHash != null ? $" [{infoHash.ToUpperInvariant()}]" : "")}");
    private string MovieFile(string titleId) => Path.Combine(moviesTitlesDirectory, $"{titleId}.json");
    private string SeriesFile(string titleId) => Path.Combine(seriesTitlesDirectory, $"{titleId}.json");

    public class Config
    {
        public string RootDirectory { get; set; } = "";
    }

    private readonly IDictionary<string, DownloadTracker> trackers = new Dictionary<string, DownloadTracker>();
    private readonly IDictionary<string, TorrentManager> managers = new Dictionary<string, TorrentManager>();
    private readonly ClientEngine torrentEngine = new ClientEngine(
        new EngineSettingsBuilder
        {
            CacheDirectory = Path.Combine(config.Value.RootDirectory, ".torrents"),
        }.ToSettings());

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, hostApplicationLifetime.ApplicationStopping);
        var token = linkedCts.Token;

        logger.LogInformation("Download Manager starting up from {RootDirectory}", config.Value.RootDirectory);

        EnsureDirectories();
        try
        {
            while (!token.IsCancellationRequested)
            {
                await Run(token);
                await Task.Delay(TimeSpan.FromSeconds(1), token);
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

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping DownloadManagementService...");
        await base.StopAsync(cancellationToken);
    }

    private void EnsureDirectories()
    {
        Directory.CreateDirectory(binDirectory);
        Directory.CreateDirectory(completedDirectory);
        Directory.CreateDirectory(fastresumeDirectory);
        Directory.CreateDirectory(metadataDirectory);
        Directory.CreateDirectory(moviesDirectory);
        Directory.CreateDirectory(moviesTitlesDirectory);
        Directory.CreateDirectory(seriesDirectory);
        Directory.CreateDirectory(seriesTitlesDirectory);
        Directory.CreateDirectory(titlesDirectory);
    }

    private async Task Run(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogDebug("Running...");
            await WatchTorrents(cancellationToken);
            await RestartFailedTorrentsAsync(cancellationToken);
            await DetectSavedDownloadsAsync(cancellationToken);
            await DownloadTorrentFilesAsync(cancellationToken);
            await AddTorrents(cancellationToken);
            await StopTorrents(cancellationToken);
            await FinalizeTorrents(cancellationToken);
            await SortFiles(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
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
                logger.LogInformation("Stopping torrent manager for {InfoHash}...", infoHash);
                await manager.StopAsync();
                await manager.SaveFastResumeAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error stopping torrent manager for {InfoHash}: {Reason}", infoHash, ex.Message);
            }
        }

        try
        {
            await torrentEngine.StopAllAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error stopping torrent engine: {Reason}", ex.Message);
        }
        torrentEngine.Dispose();
        logger.LogInformation("DownloadManagementService has been stopped and cleaned up.");
    }
}
