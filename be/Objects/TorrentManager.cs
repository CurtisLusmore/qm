using qm.Dtos;

namespace qm.Objects;

/// <summary>
/// Active object which manages a torrent
/// </summary>
public class TorrentManager
{
    private readonly string name;
    private readonly string downloadDirectory;
    private readonly string completeDirectory;
    private readonly string fastResumeFile;
    private readonly MonoTorrent.Client.ClientEngine engine;
    private readonly ILogger<TorrentManager> logger;
    private MonoTorrent.Client.TorrentManager? manager;
    private State state;
    private DownloadSpeedTimer timer = new DownloadSpeedTimer();

    /// <summary>
    /// Create a <see cref="TorrentManager"/>
    /// </summary>
    /// <param name="infoHash">The info hash</param>
    /// <param name="name">The name</param>
    /// <param name="directoryRoot">The directory in which to download files</param>
    /// <param name="engine">The torrent engine</param>
    /// <param name="logger">A logger</param>
    public TorrentManager(
        string infoHash,
        string name,
        string directoryRoot,
        MonoTorrent.Client.ClientEngine engine,
        ILogger<TorrentManager> logger)
    {
        InfoHash = infoHash;
        this.name = name;
        downloadDirectory = Path.Join(directoryRoot, ".torrent");
        completeDirectory = directoryRoot;
        fastResumeFile = Path.Join(downloadDirectory, "fastresume", $"{InfoHash}.fastresume");
        this.engine = engine;
        this.logger = logger;
    }

    /// <summary>
    /// The info hash
    /// </summary>
    public string InfoHash { get; }

    /// <summary>
    /// Start downloading the torrent
    /// </summary>
    /// <returns>A task which completes when the torrent has started downloading</returns>
    public async Task StartAsync()
    {
        state = State.Initializing;
        var magnetLink = new MonoTorrent.MagnetLink(MonoTorrent.InfoHash.FromHex(InfoHash));
        manager = await engine.AddAsync(magnetLink, downloadDirectory);
        if (MonoTorrent.Client.FastResume.TryLoad(fastResumeFile, out var fastResume))
        {
            await manager.LoadFastResumeAsync(fastResume);
            logger.LogInformation("Loaded fastresume for {infoHash} from {path}", InfoHash, fastResumeFile);
        }
        await manager.StartAsync();
        logger.LogInformation("Started torrent {infoHash}", InfoHash);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Task.Run(async () =>
        {
            await manager.WaitForMetadataAsync();
            logger.LogInformation("Downloaded metadata for \"{name}\" ({infoHash})", manager.Name, InfoHash);
            state = State.Downloading;
        });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }

    /// <summary>
    /// Monitor the torrent and perform operations relevant to its status
    /// </summary>
    /// <returns>A task which completes when any actions have been performed</returns>
    public async Task MonitorAsync()
    {
        logger.LogTrace("Torrent {infoHash} is {state} at {progress:F1}%", InfoHash, state, manager!.PartialProgress);
        switch (state)
        {
            case State.Downloading:
                if (manager.PartialProgress == 100.0)
                {
                    state = State.Completing;
                }
                else
                {
                    timer.Pulse(manager.Files.Sum(MonoTorrent.ITorrentFileInfoExtensions.BytesDownloaded));
                }
                break;

            case State.Paused:
                timer.Reset();
                break;

            case State.Completing:
                timer.Reset();
                await manager!.StopAsync();
                logger.LogDebug("Stopped {infoHash}", InfoHash);
                MoveFiles();
                DeleteFiles();
                DeleteFastResume();
                await engine.RemoveAsync(manager);
                logger.LogDebug("Removed {infoHash}", InfoHash);
                state = State.Complete;
                break;

            case State.Removing:
                timer.Reset();
                await manager!.StopAsync();
                logger.LogDebug("Stopped {infoHash}", InfoHash);
                DeleteFiles();
                DeleteFastResume();
                await engine.RemoveAsync(manager);
                logger.LogDebug("Removed {infoHash}", InfoHash);
                state = State.Removed;
                break;

            case State.Error:
                await manager!.StopAsync();
                logger.LogDebug("Stopped {infoHash}", InfoHash);
                await manager.StartAsync();
                logger.LogDebug("Started {infoHash}", InfoHash);
                state = State.Downloading;
                break;
        }
    }

    /// <summary>
    /// Stop a torrent, archiving completed torrents or removing in-progress torrents
    /// </summary>
    /// <returns>A task which completes when the torrent has been stopped and archived/removed</returns>
    public Task StopAsync()
    {
        switch (state)
        {
            case State.Initializing:
            case State.Downloading:
            case State.Paused:
                state = State.Removing;
                return WaitFor(State.Removed);

            case State.Complete:
                return Task.CompletedTask;

            default:
                throw new InvalidOperationException($"Cannot stop from state {state}");
        }
    }

    /// <summary>
    /// Patch a torrent
    /// </summary>
    /// <param name="patch">The patch to apply</param>
    /// <returns>A task which completes when the torrent has been updated</returns>
    public async Task PatchAsync(TorrentPatch patch)
    {
        switch ((state, patch.State))
        {
            case (State.Paused, PatchState.Downloading):
                await manager!.StartAsync();
                logger.LogInformation("Started {infoHash}", InfoHash);
                state = State.Downloading;
                break;

            case (State.Downloading, PatchState.Paused):
                await manager!.PauseAsync();
                logger.LogInformation("Paused {infoHash}", InfoHash);
                state = State.Paused;
                break;
        }

        foreach (var file in patch.Files ?? [])
        {
            await manager!.SetFilePriorityAsync(
                manager.Files.Single(torrentFile => torrentFile.Path == file.Path),
                file.Priority switch
                {
                    Priority.Skip => MonoTorrent.Priority.DoNotDownload,
                    Priority.Normal => MonoTorrent.Priority.Normal,
                    Priority.High => MonoTorrent.Priority.High,
                    _ => throw new InvalidOperationException($"Invalid priority {file.Priority}"),
                });
            logger.LogInformation("Set priority in {infoHash} for \"{path}\" to {priority}", InfoHash, file.Path, file.Priority);
        }
    }

    /// <summary>
    /// Save fast resume data
    /// </summary>
    /// <returns>A task which completes when the data has been saved</returns>
    public async Task SaveFastResumeAsync()
    {
        if (manager!.HasMetadata)
        {
            var fastResume = await manager!.SaveFastResumeAsync();
            await File.WriteAllBytesAsync(fastResumeFile, fastResume.Encode());
            logger.LogInformation("Saved fastresume for {infoHash} to {path}", InfoHash, fastResumeFile);
        }
    }

    /// <summary>
    /// Cast to a <see cref="Torrent"/>
    /// </summary>
    /// <param name="torrent">The torrent to cast</param>
    public static implicit operator Torrent(TorrentManager torrent) => torrent.Convert();

    private async Task WaitFor(State target)
    {
        while (state != target) await Task.Delay(1_000);
    }

    private void MoveFiles()
    {
        var root = Path.Join(completeDirectory, manager!.Name);
        foreach (var file in manager.Files)
        {
            if (file.Priority != MonoTorrent.Priority.DoNotDownload)
            {
                var src = file.FullPath;
                var dst = Path.Join(root, file.Path);
                Directory.CreateDirectory(Path.GetDirectoryName(dst)!);
                File.Move(src, dst);
                logger.LogDebug("Moved \"{source}\" to \"{destination}\"", src, dst);
            }
        }
    }

    private void DeleteFiles()
    {
        var root = Path.Join(downloadDirectory, manager!.Name);
        Directory.Delete(root, true);
        logger.LogDebug("Deleted \"{path}\"", root);
    }

    private void DeleteFastResume()
    {
        if (File.Exists(fastResumeFile))
        {
            File.Delete(fastResumeFile);
            logger.LogDebug("Deleted \"{path}\"", fastResumeFile);
        }
    }

    private Torrent Convert()
    {
        var files = manager!.Files
            .Select(Convert)
            .OrderBy(file => file.Path)
            .ToArray();
        var downloadingFiles = files.Where(file => file.Priority != Priority.Skip);

        var downloadedBytes = downloadingFiles.Sum(file => file.DownloadedBytes);
        var targetBytes = downloadingFiles.Sum(file => file.SizeBytes);
        var sizeBytes = files.Sum(file => file.SizeBytes);

        var partialProgressPercent = targetBytes == 0 ? 0 : 100.0m * downloadedBytes / targetBytes;
        var targetPercent = sizeBytes == 0 ? 0 : 100.0m * targetBytes / sizeBytes;
        var progressPercent = sizeBytes == 0 ? 0 : 100.0m * downloadedBytes / sizeBytes;

        return new Torrent(
            InfoHash,
            manager.Torrent?.Name ?? name,
            state,
            manager.Peers.Seeds,
            downloadedBytes,
            targetBytes,
            sizeBytes,
            partialProgressPercent,
            targetPercent,
            progressPercent,
            timer.BytesPerSecond,
            files);
    }

    private static TorrentFile Convert(MonoTorrent.ITorrentManagerFile file)
    {
        var downloadedBytes = MonoTorrent.ITorrentFileInfoExtensions.BytesDownloaded(file);
        var sizeBytes = file.Length;
        var progressPercent = sizeBytes == 0 ? 0 : 100.0m * downloadedBytes / sizeBytes;
        return new TorrentFile(
            file.Path,
            downloadedBytes,
            sizeBytes,
            progressPercent,
            file.Priority switch
            {
                MonoTorrent.Priority.DoNotDownload => Priority.Skip,
                MonoTorrent.Priority.Lowest => Priority.Normal,
                MonoTorrent.Priority.Low => Priority.Normal,
                MonoTorrent.Priority.Normal => Priority.Normal,
                MonoTorrent.Priority.High => Priority.High,
                MonoTorrent.Priority.Highest => Priority.High,
                MonoTorrent.Priority.Immediate => Priority.High,
                _ => throw new ArgumentOutOfRangeException(nameof(file), $"Unknown priority {file.Priority}"),
            });
    }
}
