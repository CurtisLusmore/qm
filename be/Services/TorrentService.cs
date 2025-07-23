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

    private readonly Dictionary<string, TorrentManager> torrents = [];

    /// <summary>
    /// Get all saved torrents
    /// </summary>
    /// <returns>The saved torrents</returns>
    public IEnumerable<Dtos.Torrent> GetTorrents()
        => torrents
            .Select(kv => Dtos.Torrent.ConvertFrom(kv.Key, kv.Value))
            .ToArray();

    /// <summary>
    /// Get a saved torrent
    /// </summary>
    /// <param name="infoHash">The info hash of the torrent to get</param>
    /// <returns>The saved torrent</returns>
    public Dtos.Torrent? GetTorrent(string infoHash)
    {
        if (!torrents.TryGetValue(infoHash, out var torrent))
        {
            logger.LogWarning("Torrent {infoHash} not found", infoHash);
            return null;
        }

        var response = Dtos.Torrent.ConvertFrom(infoHash, torrent);
        return response;
    }

    /// <summary>
    /// Save a torrent to start downloading
    /// </summary>
    /// <param name="infoHash">The info hash of the torrent to save</param>
    /// <returns>Whether the torrent could be saved</returns>
    public bool SaveTorrent(string infoHash)
    {
        if (torrents.ContainsKey(infoHash))
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
        if (!torrents.TryGetValue(infoHash, out var torrent))
        {
            logger.LogWarning("Torrent {infoHash} not found", infoHash);
            return false;
        }

        Task.Run(() => DeleteTorrentAsync(infoHash, torrent));

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
        if (!torrents.TryGetValue(infoHash, out var torrent))
        {
            logger.LogWarning("Torrent {infoHash} not found", infoHash);
            return false;
        }

        Task.Run(() => UpdateTorrentAsync(infoHash, torrent, patch));

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
                logger.LogError(ex, "An error occurred loading torrent {file}: {reason}", file, ex.Message);
            }
        }));

        // Monitor torrents
        while (!stoppingToken.IsCancellationRequested)
        {
            var torrentsArray = torrents.ToArray();
            foreach (var kv in torrentsArray)
            {
                try
                {
                    var infoHash = kv.Key;
                    var torrent = kv.Value;
                    var progress = torrent.PartialProgress;
                    switch (torrent.State)
                    {
                        case TorrentState.Seeding:
                            progress = 100;
                            break;
                        case TorrentState.Error:
                            await torrent.StopAsync();
                            await torrent.StartAsync();
                            break;
                        case TorrentState.Stopped:
                            continue;
                    }
                    if (progress == 100) await CompleteTorrentAsync(infoHash, torrent);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while monitoring torrent {infoHash}: {reason}", kv.Key, ex.Message);
                }
            }
            await Task.Delay(1_000, stoppingToken);
        }
    }

    private async Task<TorrentManager> DownloadOrLoadTorrentAsync(string infoHash)
    {
        try
        {
            if (torrents.TryGetValue(infoHash, out var torrent)) return torrent;

            var magnetLink = new MagnetLink(InfoHash.FromHex(infoHash));
            var manager = await engine!.AddAsync(magnetLink, InProgressDirectory);
            await manager.StartAsync();
            torrents[infoHash] = manager;
            logger.LogInformation("Downloading {infoHash} to \"{path}\"", infoHash, InProgressDirectory);
            return manager;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred downloading {infoHash}: {reason}", infoHash, ex.Message);
            throw;
        }
    }

    private async Task DeleteTorrentAsync(string infoHash, TorrentManager torrent)
    {
        try
        {
            if (torrent.State != TorrentState.Stopped)
            {
                await torrent.StopAsync();

                foreach (var file in torrent.Files)
                {
                    File.Delete(file.FullPath);
                    logger.LogDebug("Deleted \"{path}\"", file.FullPath);
                }

                DeleteDirectories(torrent);

                var torrentFile = Path.Join(MetadataDirectory, $"{infoHash}.torrent");
                File.Delete(torrentFile);
                logger.LogDebug("Deleted \"{path}\"", torrentFile);

                await engine!.RemoveAsync(torrent);
            }

            torrents.Remove(infoHash);

            logger.LogInformation("Removed torrent \"{name} ({infoHash})", torrent.Name, infoHash);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred removing {infoHash}: {reason}", infoHash, ex.Message);
            throw;
        }
    }

    private async Task UpdateTorrentAsync(string infoHash, TorrentManager torrent, TorrentPatch patch)
    {
        try
        {
            switch (patch.State)
            {
                case PatchState.Downloading:
                    await torrent.StartAsync();
                    logger.LogInformation("Resuming torrent \"{name}\" ({infoHash})", torrent.Name, infoHash);
                    break;
                case PatchState.Paused:
                    await torrent.PauseAsync();
                    logger.LogInformation("Pausing torrent \"{name}\" ({infoHash})", torrent.Name, infoHash);
                    break;
            }

            foreach (var file in patch.Files ?? [])
            {
                await torrent.SetFilePriorityAsync(
                    torrent.Files.Single(torrentFile => torrentFile.Path == file.Path),
                    Convert(file.Priority));
                logger.LogInformation("Setting priority to {priority} for \"{path}\" of torrent \"{name}\" ({infoHash})", file.Priority, file.Path, torrent.Name, infoHash);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred updating {infoHash}: {reason}", infoHash, ex.Message);
            throw;
        }
    }

    private async Task CompleteTorrentAsync(string infoHash, TorrentManager torrent)
    {
        try
        {
            var newPath = Path.Join(CompletedDirectory, torrent.Name);
            await torrent.StopAsync();
            await torrent.MoveFilesAsync(newPath, true);
            logger.LogDebug("Moved files to \"{path}\"", newPath);
            DeleteUnwantedFiles(torrent);
            DeleteDirectories(torrent);
            await engine!.RemoveAsync(torrent);

            logger.LogInformation("Completed torrent \"{name} ({infoHash})", torrent.Name, infoHash);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred completing {infoHash}: {reason}", infoHash, ex.Message);
            throw;
        }
    }

    private void DeleteUnwantedFiles(TorrentManager torrent)
    {
        foreach (var file in torrent.Files)
        {
            if (file.Priority == MonoTorrent.Priority.DoNotDownload)
            {
                File.Delete(file.FullPath);
                logger.LogDebug("Deleted unwanted file {path}", file.FullPath);
            }
        }
    }

    private void DeleteDirectories(TorrentManager torrent)
    {
        var directory = Path.Join(InProgressDirectory, torrent.Name);
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
