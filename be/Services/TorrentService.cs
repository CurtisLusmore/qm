using Microsoft.Extensions.Options;
using qm.Dtos;
using qm.Objects;
using qm.Repositories;

namespace qm.Services;

/// <summary>
/// Torrent service
/// </summary>
public class TorrentService : BackgroundService
{
    private readonly string directoryRoot;
    private readonly string downloadDirectory;
    private readonly string metadataDirectory;
    private readonly ILogger<TorrentService> logger;
    private readonly ILoggerFactory loggerFactory;
    private Repository<TorrentManager> repository = new Repository<TorrentManager>();
    private MonoTorrent.Client.ClientEngine? engine;

    /// <summary>
    /// Torrent service
    /// </summary>
    /// <param name="config">Configuration</param>
    /// <param name="logger">Logger</param>
    /// <param name="loggerFactory">Logger factory</param>
    public TorrentService(
        IOptions<TorrentServiceConfig> config,
        ILogger<TorrentService> logger,
        ILoggerFactory loggerFactory)
    {
        directoryRoot = config.Value.DirectoryRoot;
        downloadDirectory = Path.Join(directoryRoot, ".torrent");
        metadataDirectory = Path.Join(downloadDirectory, "metadata");
        this.logger = logger;
        this.loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Get all saved torrents
    /// </summary>
    /// <returns>The saved torrents</returns>
    public IEnumerable<Torrent> GetTorrents()
        => repository
            .List()
            .Select(torrent => (Torrent)torrent)
            .ToArray();

    /// <summary>
    /// Get a saved torrent
    /// </summary>
    /// <param name="infoHash">The info hash of the torrent to get</param>
    /// <returns>The saved torrent</returns>
    public Torrent? GetTorrent(string infoHash)
    {
        if (!repository.TryGet(infoHash, out var torrent))
        {
            logger.LogWarning("Torrent {infoHash} not found", infoHash);
            return null;
        }

        return torrent;
    }

    /// <summary>
    /// Save a torrent to start downloading
    /// </summary>
    /// <param name="details">The details of the torrent to save</param>
    /// <returns>Whether the torrent could be saved</returns>
    public bool SaveTorrent(SaveTorrent details)
    {
        var (infoHash, name) = details;
        if (repository.Contains(infoHash))
        {
            logger.LogWarning("Torrent {infoHash} already saved", infoHash);
            return false;
        }

        var torrent = new TorrentManager(
            infoHash,
            name,
            directoryRoot,
            engine!,
            loggerFactory.CreateLogger<TorrentManager>());

        Task.Run(async () =>
        {
            try
            {
                await torrent.StartAsync();
                repository.Add(infoHash, torrent);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred saving torrent {infoHash}: {reason}", infoHash, ex);
            }
        });

        return true;
    }

    /// <summary>
    /// Remove a torrent
    /// </summary>
    /// <param name="infoHash">The info hash of the torrent to remove</param>
    /// <returns>Whether the request could be accepted</returns>
    public bool RemoveTorrent(string infoHash)
    {
        if (!repository.TryGet(infoHash, out var torrent))
        {
            logger.LogWarning("Torrent {infoHash} not found", infoHash);
            return false;
        }

        Task.Run(async () =>
        {
            try
            {
                await torrent.StopAsync();
                repository.Remove(infoHash);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred removing torrent {infoHash}: {reason}", infoHash, ex);
            }
        });

        return true;
    }

    /// <summary>
    /// Update the state or priority of a torrent or its files
    /// </summary>
    /// <param name="infoHash">The info hash of the torrent to update</param>
    /// <param name="patch">The updates to make</param>
    /// <returns>Whether the request could be accepted</returns>
    public bool UpdateTorrent(string infoHash, TorrentPatch patch)
    {
        if (!repository.TryGet(infoHash, out var torrent))
        {
            logger.LogWarning("Torrent {infoHash} not found", infoHash);
            return false;
        }

        Task.Run(async () =>
        {
            try
            {
                await torrent.PatchAsync(patch);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred updating torrent {infoHash}: {reason}", infoHash, ex);
            }
        });

        return true;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await RunAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            try
            {
                await CleanupAsync();
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "An error occurred while cleaning up torrent service: {reason}", ex);
                throw;
            }
        }
    }

    private async Task RunAsync(CancellationToken stoppingToken)
    {
        engine = new MonoTorrent.Client.ClientEngine(
            new MonoTorrent.Client.EngineSettingsBuilder
            {
                AutoSaveLoadMagnetLinkMetadata = true,
                CacheDirectory = downloadDirectory,
            }.ToSettings());

        // Initialize torrents
        logger.LogInformation("Initializing torrents from \"{path}\"", metadataDirectory);
        Directory.CreateDirectory(metadataDirectory);
        var files = Directory.GetFiles(metadataDirectory, "*.torrent");
        var infoHashes = files
            .Select(file => Path.GetFileNameWithoutExtension(file)!)
            .ToArray();

        foreach (var infoHash in infoHashes)
        {
            try
            {
                var torrent = new TorrentManager(infoHash, "", directoryRoot, engine, loggerFactory.CreateLogger<TorrentManager>());
                await torrent.StartAsync();
                repository.Add(infoHash, torrent);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred loading torrent {infoHash}: {reason}", infoHash, ex);
            }
        }

        // Monitor torrents
        while (!stoppingToken.IsCancellationRequested)
        {
            var torrents = repository.List();
            if (!torrents.Any())
            {
                logger.LogTrace("No torrents to monitor");
            }
            foreach (var torrent in torrents)
                {
                    try
                    {
                        await torrent.MonitorAsync();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred while monitoring torrent {infoHash}: {reason}", torrent.InfoHash, ex);
                    }
                }
            await Task.Delay(1_000, stoppingToken);
            stoppingToken.ThrowIfCancellationRequested();
        }
    }

    private async Task CleanupAsync()
    {
        foreach (var torrent in repository.List())
        {
            await torrent.SaveFastResumeAsync();
        }
        await engine!.StopAllAsync();
        logger.LogInformation("Stopped all torrents");
        engine.Dispose();
        logger.LogInformation("Shut down engine");
    }
}

/// <summary>
/// Config for <see cref="TorrentService" />
/// </summary>
public class TorrentServiceConfig
{
    /// <summary>
    /// Section name in Configuration
    /// </summary>
    public const string SectionName = "TorrentService";

    /// <summary>
    /// The root directory to save torrents
    /// </summary>
    public string DirectoryRoot { get; init; } = Directory.GetCurrentDirectory();
}
