using Microsoft.Extensions.Options;
using MonoTorrent;
using MonoTorrent.Client;
using qm.Dtos;

namespace qm.Services;

/// <summary>
/// Torrent service
/// </summary>
public class TorrentService(
    string directoryRoot,
    ILogger<TorrentService> logger) : BackgroundService
{
    /// <summary>
    /// Torrent service
    /// </summary>
    /// <param name="config">Configuration</param>
    /// <param name="logger">Logger</param>
    public TorrentService(
        IOptions<TorrentServiceConfig> config,
        ILogger<TorrentService> logger)
        : this(config.Value.DirectoryRoot, logger)
    {
    }

    private string InProgressDirectory => Path.Join(directoryRoot, ".torrent");
    private string MetadataDirectory => Path.Join(InProgressDirectory, "metadata");
    private string CompletedDirectory => directoryRoot;

    private ClientEngine? engine;

    private readonly List<(string InfoHash, TorrentManager Manager)> managers = [];

    /// <summary>
    /// Get all saved torrents
    /// </summary>
    /// <returns>The saved torrents</returns>
    public IEnumerable<Dtos.Torrent> GetTorrents()
        => managers
            .Select(pair => Dtos.Torrent.ConvertFrom(pair.InfoHash, pair.Manager))
            .ToArray();

    /// <summary>
    /// Get a saved torrent
    /// </summary>
    /// <param name="infoHash">The info hash of the torrent to get</param>
    /// <returns>The saved torrent</returns>
    public Dtos.Torrent? GetTorrent(string infoHash)
    {
        var manager = managers.SingleOrDefault(pair => pair.InfoHash == infoHash).Manager;
        if (manager is null)
        {
            logger.LogWarning("Torrent {infoHash} not found", infoHash);
            return null;
        }

        var response = Dtos.Torrent.ConvertFrom(infoHash, manager);
        return response;
    }

    /// <summary>
    /// Save a torrent to start downloading
    /// </summary>
    /// <param name="infoHash">The info hash of the torrent to save</param>
    /// <returns>Whether the torrent could be saved</returns>
    public bool SaveTorrent(string infoHash)
    {
        if (managers.Any(manager => manager.InfoHash == infoHash))
        {
            logger.LogWarning("Torrent {infoHash} already saved", infoHash);
            return false;
        }

        Task.Run(() => DownloadOrLoadTorrentAsync(infoHash));

        return true;
    }

    /// <summary>
    /// Remove a torrent
    /// </summary>
    /// <param name="infoHash">The info hash of the torrent to remove</param>
    /// <returns>Whether the request could be accepted</returns>
    public bool RemoveTorrent(string infoHash)
    {
        var manager = managers.SingleOrDefault(pair => pair.InfoHash == infoHash).Manager;
        if (manager is null)
        {
            logger.LogWarning("Torrent {infoHash} not found", infoHash);
            return false;
        }

        Task.Run(() => DeleteTorrentAsync(infoHash, manager));

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
        var manager = managers.SingleOrDefault(pair => pair.InfoHash == infoHash).Manager;
        if (manager is null)
        {
            logger.LogWarning("Torrent {infoHash} not found", infoHash);
            return false;
        }

        Task.Run(() => UpdateTorrentAsync(infoHash, manager, patch));

        return true;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        engine = new ClientEngine(
            new EngineSettingsBuilder
            {
                AutoSaveLoadMagnetLinkMetadata = true,
                CacheDirectory = InProgressDirectory,
            }.ToSettings());

        // Initialize torrents
        logger.LogInformation("Initializing torrents from \"{path}\"", MetadataDirectory);
        Directory.CreateDirectory(MetadataDirectory);
        var files = Directory.GetFiles(MetadataDirectory, "*.torrent");
        await Task.WhenAll(files.Select(async file =>
        {
            try
            {
                var infoHash = Path.GetFileNameWithoutExtension(file);
                await DownloadOrLoadTorrentAsync(infoHash);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred loading torrent {file}: {reason}", file, ex);
            }
        }));

        // Monitor torrents
        while (!stoppingToken.IsCancellationRequested)
        {
            var managersArray = managers.ToArray();
            foreach (var (infoHash, manager) in managersArray)
            {
                try
                {
                    var progress = manager.PartialProgress;
                    switch (manager.State)
                    {
                        case TorrentState.Seeding:
                            progress = 100;
                            break;
                        case TorrentState.Error:
                            await manager.StopAsync();
                            await manager.StartAsync();
                            break;
                        case TorrentState.Stopped:
                            continue;
                    }
                    if (progress == 100) await CompleteTorrentAsync(infoHash, manager);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while monitoring torrent {infoHash}: {reason}", infoHash, ex);
                }
            }
            await Task.Delay(1_000, stoppingToken);
        }
    }

    private async Task<TorrentManager> DownloadOrLoadTorrentAsync(string infoHash)
    {
        try
        {
            var manager = managers.SingleOrDefault(pair => pair.InfoHash == infoHash).Manager;
            if (manager is not null) return manager;

            var magnetLink = new MagnetLink(InfoHash.FromHex(infoHash));
            manager = await engine!.AddAsync(magnetLink, InProgressDirectory);
            await manager.StartAsync();
            managers.Add((infoHash, manager));
            logger.LogInformation("Downloading {infoHash} to \"{path}\"", infoHash, InProgressDirectory);
            return manager;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred downloading {infoHash}: {reason}", infoHash, ex);
            throw;
        }
    }

    private async Task DeleteTorrentAsync(string infoHash, TorrentManager manager)
    {
        try
        {
            if (manager.State != TorrentState.Stopped)
            {
                await manager.StopAsync();

                foreach (var file in manager.Files)
                {
                    if (File.Exists(file.FullPath)) File.Delete(file.FullPath);
                    logger.LogDebug("Deleted \"{path}\"", file.FullPath);
                }

                DeleteDirectories(manager);

                var torrentFile = Path.Join(MetadataDirectory, $"{infoHash}.torrent");
                if (File.Exists(torrentFile)) File.Delete(torrentFile);
                logger.LogDebug("Deleted \"{path}\"", torrentFile);

                await engine!.RemoveAsync(manager);
            }

            managers.Remove((infoHash, manager));

            logger.LogInformation("Removed torrent \"{name} ({infoHash})", manager.Name, infoHash);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred removing {infoHash}: {reason}", infoHash, ex);
            throw;
        }
    }

    private async Task UpdateTorrentAsync(string infoHash, TorrentManager manager, TorrentPatch patch)
    {
        try
        {
            switch (patch.State)
            {
                case PatchState.Downloading:
                    await manager.StartAsync();
                    logger.LogInformation("Resuming torrent \"{name}\" ({infoHash})", manager.Name, infoHash);
                    break;
                case PatchState.Paused:
                    await manager.PauseAsync();
                    logger.LogInformation("Pausing torrent \"{name}\" ({infoHash})", manager.Name, infoHash);
                    break;
            }

            foreach (var file in patch.Files ?? [])
            {
                await manager.SetFilePriorityAsync(
                    manager.Files.Single(torrentFile => torrentFile.Path == file.Path),
                    Convert(file.Priority));
                logger.LogInformation("Setting priority to {priority} for \"{path}\" of torrent \"{name}\" ({infoHash})", file.Priority, file.Path, manager.Name, infoHash);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred updating {infoHash}: {reason}", infoHash, ex);
            throw;
        }
    }

    private async Task CompleteTorrentAsync(string infoHash, TorrentManager manager)
    {
        try
        {
            var newPath = Path.Join(CompletedDirectory, manager.Name);
            await manager.StopAsync();
            await manager.MoveFilesAsync(newPath, true);
            logger.LogDebug("Moved files to \"{path}\"", newPath);
            DeleteUnwantedFiles(manager);
            DeleteDirectories(manager);
            await engine!.RemoveAsync(manager);

            logger.LogInformation("Completed torrent \"{name} ({infoHash})", manager.Name, infoHash);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred completing {infoHash}: {reason}", infoHash, ex);
            throw;
        }
    }

    private void DeleteUnwantedFiles(TorrentManager manager)
    {
        foreach (var file in manager.Files)
        {
            if (file.Priority == MonoTorrent.Priority.DoNotDownload)
            {
                File.Delete(file.FullPath);
                logger.LogDebug("Deleted unwanted file {path}", file.FullPath);
            }
        }
    }

    private void DeleteDirectories(TorrentManager manager)
    {
        var directory = Path.Join(InProgressDirectory, manager.Name);
        if (Directory.Exists(directory)) Directory.Delete(directory, true);
    }

    private static MonoTorrent.Priority Convert(Dtos.Priority priority) => priority switch
    {
        Dtos.Priority.Skip => MonoTorrent.Priority.DoNotDownload,
        Dtos.Priority.Normal => MonoTorrent.Priority.Normal,
        Dtos.Priority.High => MonoTorrent.Priority.High,
        _ => throw new ArgumentOutOfRangeException(nameof(priority)),
    };
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
