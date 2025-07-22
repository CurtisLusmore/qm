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
        => torrents.Select(kv => Convert(kv.Key, kv.Value)).OrderByDescending(torrent => torrent.ProgressPercent).ToArray();

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

        var response = Convert(infoHash, torrent);
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
            try
            {
                var torrentsArray = torrents.ToArray();
                foreach (var kv in torrentsArray)
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
                    }
                    if (progress == 100) await CompleteTorrentAsync(infoHash, torrent);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while monitoring torrents: {reason}", ex.Message);
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
            await torrent.StopAsync();

            var directories = new HashSet<string>();
            foreach (var file in torrent.Files)
            {
                File.Delete(file.FullPath);
                logger.LogInformation("Deleted \"{path}\"", file.FullPath);

                var directory = Path.GetDirectoryName(file.FullPath)!;
                if (directory != InProgressDirectory)
                {
                    var relativePath = Path.GetRelativePath(InProgressDirectory, directory);
                    var firstChild = Path.Join(InProgressDirectory, relativePath.Split(Path.DirectorySeparatorChar).First());
                    directories.Add(firstChild);
                }
            }

            foreach (var directory in directories)
            {
                Directory.Delete(directory);
                logger.LogInformation("Deleted \"{path}\"", directory);
            }

            var torrentFile = Path.Join(MetadataDirectory, $"{infoHash}.torrent");
            File.Delete(torrentFile);
            logger.LogInformation("Deleted \"{path}\"", torrentFile);

            await engine!.RemoveAsync(torrent);
            torrents.Remove(infoHash);

            logger.LogInformation("Removed torrent {name} ({infoHash})", torrent.Name, infoHash);
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
                    logger.LogInformation("Resuming torrent {name} ({infoHash})", torrent.Name, infoHash);
                    break;
                case PatchState.Paused:
                    await torrent.PauseAsync();
                    logger.LogInformation("Pausing torrent {name} ({infoHash})", torrent.Name, infoHash);
                    break;
            }

            foreach (var file in patch.Files ?? [])
            {
                await torrent.SetFilePriorityAsync(
                    torrent.Files.Single(torrentFile => torrentFile.Path == file.Path),
                    Convert(file.Priority));
                logger.LogInformation("Setting priority to {priority} for \"{path}\" of torrent {name} ({infoHash})", file.Priority, file.Path, torrent.Name, infoHash);
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
            logger.LogInformation("Moved files to \"{path}\"", newPath);
            await engine!.RemoveAsync(torrent);
            torrents.Remove(infoHash);

            logger.LogInformation("Completed torrent {name} ({infoHash})", torrent.Name, infoHash);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred completing {infoHash}: {reason}", infoHash, ex.Message);
            throw;
        }
    }

    private static Dtos.Torrent Convert(string infoHash, TorrentManager torrent)
        => new Dtos.Torrent(
            infoHash,
            torrent.Torrent?.Name ?? "Loading...",
            Convert(torrent.State),
            torrent.Peers.Seeds,
            torrent.Files.Sum(ITorrentFileInfoExtensions.BytesDownloaded),
            torrent.Torrent?.Size ?? 0,
            torrent.Files.Count,
            torrent.Files.Select(Convert).OrderBy(file => file.Path).ToArray());

    private static Dtos.TorrentFile Convert(ITorrentManagerFile file)
        => new Dtos.TorrentFile(
            file.Path,
            ITorrentFileInfoExtensions.BytesDownloaded(file),
            file.Length,
            Convert(file.Priority));

    private static State Convert(TorrentState state) => state switch
    {
        TorrentState.Stopped => State.Stopped,
        TorrentState.Paused => State.Paused,
        TorrentState.Starting => State.Hashing,
        TorrentState.Downloading => State.Downloading,
        TorrentState.Seeding => State.Stopped,
        TorrentState.Hashing => State.Hashing,
        TorrentState.HashingPaused => State.Paused,
        TorrentState.Stopping => State.Stopped,
        TorrentState.Error => State.Error,
        TorrentState.Metadata => State.Hashing,
        TorrentState.FetchingHashes => State.Hashing,
        _ => throw new ArgumentOutOfRangeException(nameof(state)),
    };

    private static Dtos.Priority Convert(MonoTorrent.Priority priority) => priority switch
    {
        MonoTorrent.Priority.DoNotDownload => Dtos.Priority.Skip,
        MonoTorrent.Priority.Lowest => Dtos.Priority.Normal,
        MonoTorrent.Priority.Low => Dtos.Priority.Normal,
        MonoTorrent.Priority.Normal => Dtos.Priority.Normal,
        MonoTorrent.Priority.High => Dtos.Priority.High,
        MonoTorrent.Priority.Highest => Dtos.Priority.High,
        MonoTorrent.Priority.Immediate => Dtos.Priority.High,
        _ => throw new ArgumentOutOfRangeException(nameof(priority)),
    };

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
