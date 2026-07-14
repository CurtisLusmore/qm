using be.Interfaces;
using be.Models;

namespace be.Services;

public class DownloadManager
{
    public string InfoHash { get; init; }
    public MovieOrSeries Title { get; init; }
    public string Name { get; set;}
    public Download State { get; set; }

    private readonly FileService fileService;
    private readonly MonoTorrent.Client.ClientEngine torrentEngine;
    private readonly IEventStream eventStream;
    private readonly ILogger<DownloadManager> logger;
    private MonoTorrent.Client.TorrentManager? torrentManager;
    private List<FileMapping> fileMappings = new();
    private bool pauseRequested = false;
    private bool deleteRequested = false;

    private DownloadManager(
        string infoHash,
        MovieOrSeries title,
        FileService fileService,
        MonoTorrent.Client.ClientEngine torrentEngine,
        IEventStream eventStream,
        ILogger<DownloadManager> logger)
    {
        InfoHash = infoHash;
        Title = title;
        Name =  $"{Title.Name} ({Title.Year}) [{InfoHash}]";
        this.fileService = fileService;
        this.torrentEngine = torrentEngine;
        this.eventStream = eventStream;
        this.logger = logger;

        State = new Download(
            Name,
            InfoHash,
            Title,
            DownloadStatus.Received,
            null,
            [],
            0,
            0,
            0,
            0.0,
            0.0,
            0.0,
            0.0,
            0);
    }

    public static async Task<DownloadManager> AddAsync(
        string infoHash,
        MovieOrSeries title,
        FileService fileService,
        MonoTorrent.Client.ClientEngine torrentEngine,
        IEventStream eventStream,
        ILogger<DownloadManager> logger,
        CancellationToken cancellationToken)
    {
        await fileService.SaveTitleAsync(infoHash, title, cancellationToken);
        return new DownloadManager(
            infoHash,
            title,
            fileService,
            torrentEngine,
            eventStream,
            logger);
    }

    public static async Task<DownloadManager> LoadAsync(
        string infoHash,
        FileService fileService,
        MonoTorrent.Client.ClientEngine torrentEngine,
        IEventStream eventStream,
        ILogger<DownloadManager> logger,
        CancellationToken cancellationToken)
    {
        var title = await fileService.LoadTitleAsync(infoHash, cancellationToken);
        return new DownloadManager(
            infoHash,
            title,
            fileService,
            torrentEngine,
            eventStream,
            logger);
    }

    public async Task RequestPatchAsync(DownloadPatch patch)
    {
        switch (patch.Status)
        {
            case DownloadStatus.DownloadingFiles:
                pauseRequested = false;
                break;

            case DownloadStatus.DownloadPaused:
                pauseRequested = true;
                break;

            default:
                logger.LogWarning("Attempted to patch download with unsupported status {Status}", patch.Status);
                return;
        }

        await NotifyProgress();
    }

    public void SetFilePriorities()
    {
    }

    public async Task RequestDeleteAsync()
    {
        deleteRequested = true;
        if (State.Status != DownloadStatus.Completed)
        {
            State = State with { Status = DownloadStatus.Deleting };
            await NotifyProgress();
        }
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope<List<(string, object)>>([
            ( "InfoHash", InfoHash ),
            ( "Name", Title.Name ),
        ]);
        try
        {
            logger.LogInformation("Starting download");
            await NotifyAdded();
            await DownloadTorrentFileAsync(cancellationToken);
            await AddTorrentAsync(cancellationToken);
            await MapFilesAsync(cancellationToken);
            await LoadFastResumeAsync(cancellationToken);
            await StartTorrentAsync(cancellationToken);
            await LoadMetadataAsync(cancellationToken);
            await WaitForDownloadCompletionAsync(cancellationToken);
            await StopDownloadedTorrentAsync(cancellationToken);
            DeleteTorrentFiles();
            MoveFilesToCompletedDirectory(cancellationToken);
            await NotifyCompleted();
            await UpdateTitleFile(cancellationToken);
            await WaitForDismissRequestAsync(cancellationToken);
            await NotifyRemoved();
            logger.LogInformation("Download completed successfully");
        }
        catch (DeleteRequestedException)
        {
            logger.LogInformation("Deleting download");
            await NotifyProgress();
            await StopDownloadedTorrentAsync(cancellationToken, ignoreDeleteRequest: true);
            DeleteTorrentFiles(ignoreDeleteRequest: true);
            DeleteDownloadingFiles(cancellationToken);
            await NotifyRemoved();
            logger.LogInformation("Download was deleted");
        }
        catch (OperationCanceledException)
        {
            await SaveFastResumeAsync();
        }
        catch (Exception ex)
        {
            State = State with
            {
                Status = DownloadStatus.Failed,
                Error = ex.Message,  
            };
            await NotifyFailed();
            logger.LogError(ex, "Error during download: {Message}", ex.Message);
            throw;
        }
    }

    private async Task DownloadTorrentFileAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDeleteRequested();
        State = State with
        {
            Status = DownloadStatus.DownloadingTorrentFile,
            Error = null,
        };
        try
        {
            await fileService.DownloadTorrentFileAsync(InfoHash, cancellationToken);
            logger.LogInformation("Downloaded torrent file");
            await NotifyProgress();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Failed to download torrent file: {Message}", ex.Message);
        }
    }

    private async Task AddTorrentAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDeleteRequested();
        State = State with
        {
            Status = DownloadStatus.AddingTorrent,
            Error = null,
        };
        var settings = new MonoTorrent.Client.TorrentSettingsBuilder
        {
            CreateContainingDirectory = false,
        }.ToSettings();

        if (!fileService.TorrentFileExists(InfoHash))
        {
            logger.LogWarning("Torrent file does not exist. Trying magnet link instead");
            await AddFromMagnetLinkAsync();
        }
        else
        {
            try
            {
                var torrent = MonoTorrent.Torrent.Load(fileService.TorrentFile(InfoHash));
                torrentManager = await torrentEngine.AddAsync(torrent, DownloadingDirectory, settings);
            }
            catch (MonoTorrent.TorrentException ex)
            {
                logger.LogWarning(ex, "Invalid torrent file. Trying magnet link instead");
                fileService.DeleteTorrentFile(InfoHash);
                await AddFromMagnetLinkAsync();
            }
        }

        logger.LogInformation("Added torrent");
        await NotifyProgress();

        async Task AddFromMagnetLinkAsync()
        {
            var magnetLink = MonoTorrent.MagnetLink.FromUri(new Uri($"magnet:?xt=urn:btih:{InfoHash}&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969%2Fannounce&tr=udp%3A%2F%2Ftracker.opentrackr.org%3A1337%2Fannounce&tr=udp%3A%2F%2Fopen.stealth.si%3A80%2Fannounce"));
            torrentManager = await torrentEngine.AddAsync(magnetLink, DownloadingDirectory, settings);
        }
    }

    private async Task MapFilesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDeleteRequested();
        State = State with
        {
            Status = DownloadStatus.MappingFiles,
            Error = null,
        };
        fileMappings = fileService.MapFiles(Title, torrentManager!.Files).ToList();
        foreach (var (file, destinationPath, priority, _) in fileMappings)
        {
            await torrentManager!.SetFilePriorityAsync(file, MapPriority(priority));
            await torrentManager!.MoveFileAsync(file, Path.Join(DownloadingDirectory, destinationPath));
            logger.LogDebug("Mapped file: {SourcePath} -> {DestinationPath} (Priority: {Priority})", file.Path, destinationPath, priority);
        }
        State = WithProgress(State) with
        {
            Name = GetName(),
        };

        logger.LogInformation("Mapped files");
        await NotifyProgress();

        string GetName()
        {
            switch (Title.Type)
            {
                case TitleType.Series:
                    var eps = fileMappings.Where(m => m.EpisodeInfo != null).Select(m => m.EpisodeInfo!).ToList();
                    var seasons = eps.GroupBy(e => e.SeasonNumber).ToDictionary(g => g.Key, g => new HashSet<int>(g.Select(e => e.EpisodeNumber)));
                    if (seasons.Count == 0)
                    {
                        return $"{Title.Name} ({Title.Year}) [{InfoHash}]";
                    }
                    else if (seasons.Count == 1 && seasons.First().Value.Count == 1)
                    {
                        var season = seasons.First().Key;
                        var episode = seasons.First().Value.First();
                        return $"{Title.Name} ({Title.Year}) - S{season:D2}E{episode:D2}";
                    }
                    else if (seasons.Count == 1)
                    {
                        var season = seasons.First().Key;
                        return $"{Title.Name} ({Title.Year}) - S{season:D2}";
                    }
                    else
                    {
                        var lowestSeason = seasons.Keys.Min();
                        var highestSeason = seasons.Keys.Max();
                        return $"{Title.Name} ({Title.Year}) - S{lowestSeason:D2}-S{highestSeason:D2}";
                    }

                default:
                    return $"{Title.Name} ({Title.Year})";
            }
        }
    }

    private async Task LoadFastResumeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDeleteRequested();
        if (MonoTorrent.Client.FastResume.TryLoad(fileService.FastResumeFile(InfoHash), out var fastResume))
        {
            State = State with
            {
                Status = DownloadStatus.LoadingFastResume,
                Error = null,
            };
            await torrentManager!.LoadFastResumeAsync(fastResume);
            logger.LogInformation("Loaded fast resume");
            await NotifyProgress();
        }
    }

    private async Task StartTorrentAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDeleteRequested();
        State = State with
        {
            Status = DownloadStatus.StartingTorrent,
            Error = null,
        };
        await torrentManager!.StartAsync();
        logger.LogInformation("Started torrent");
        await NotifyProgress();
    }

    private async Task LoadMetadataAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDeleteRequested();
        State = State with
        {
            Status = DownloadStatus.LoadingMetadata,
            Error = null,
        };
        await torrentManager!.WaitForMetadataAsync(cancellationToken);
        logger.LogInformation("Loaded metadata");
        await NotifyProgress();
    }

    private async Task WaitForDownloadCompletionAsync(CancellationToken cancellationToken)
    {
        while (torrentManager!.State != MonoTorrent.Client.TorrentState.Seeding)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDeleteRequested();
            if (pauseRequested && torrentManager.State == MonoTorrent.Client.TorrentState.Downloading)
            {
                await torrentManager.PauseAsync();
                logger.LogInformation("Paused download");
            }
            else if (!pauseRequested && torrentManager.State == MonoTorrent.Client.TorrentState.Paused)
            {
                await torrentManager.StartAsync();
                logger.LogInformation("Resumed download");
            }
            State = WithProgress(State) with
            {
                Status = pauseRequested ? DownloadStatus.DownloadPaused : DownloadStatus.DownloadingFiles,
                Error = null,
            };
            await NotifyProgress();
            logger.LogDebug("Downloaded {Progress:P2} with {Seeds} seeds", torrentManager.Progress / 100.0, torrentManager.Peers.Seeds);
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }
        logger.LogInformation("Download completed");
        await NotifyProgress();
    }

    private async Task StopDownloadedTorrentAsync(CancellationToken cancellationToken, bool ignoreDeleteRequest = false)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!ignoreDeleteRequest) ThrowIfDeleteRequested();
        State = State with
        {
            Status = DownloadStatus.StoppingTorrent,
            Error = null,
        };
        await torrentManager!.StopAsync(TimeSpan.FromSeconds(2));
        await torrentEngine.RemoveAsync(torrentManager);
        logger.LogInformation("Stopped downloaded torrent");
        await NotifyProgress();
    }

    private void DeleteTorrentFiles(bool ignoreDeleteRequest = false)
    {
        if (!ignoreDeleteRequest) ThrowIfDeleteRequested();
        fileService.DeleteTorrentFiles(InfoHash);
        logger.LogInformation("Deleted torrent files");
    }

    private void MoveFilesToCompletedDirectory(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDeleteRequested();
        fileService.MoveCompletedFiles(InfoHash, Title, fileMappings);
        State = State with
        {
            Status = DownloadStatus.Completed,
            Error = null,
        };
        logger.LogInformation("Moved torrent files to completed directory");
    }

    private async Task UpdateTitleFile(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDeleteRequested();
        switch (Title.Type)
        {
            case TitleType.Movie:
                await fileService.MarkMovieDownloadedAsync(Title, cancellationToken);
                break;

            case TitleType.Series:
                await fileService.MarkEpisodesDownloadedAsync(
                    Title,
                    fileMappings
                        .Where(m => m.Priority != FilePriority.Skip)
                        .Where(m => m.EpisodeInfo != null)
                        .Select(m => m.EpisodeInfo!),
                    cancellationToken);
                break;
        }
    }

    private async Task WaitForDismissRequestAsync(CancellationToken cancellationToken)
    {
        while (!deleteRequested)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDeleteRequested();
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }
        logger.LogInformation("Waiting for dismiss request");
    }

    private void DeleteDownloadingFiles(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        fileService.DeleteDownloadingFiles(InfoHash, Title);
        logger.LogInformation("Deleted downloading files");
    }

    private async Task SaveFastResumeAsync()
    {
        try
        {
            await torrentManager!.SaveFastResumeAsync();
            logger.LogInformation("Saved fast resume file");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to save fast resume: {Message}", ex.Message);
        }
    }

    private string DownloadingDirectory => fileService.DownloadingDirectory(InfoHash, Title);

    private MonoTorrent.Priority MapPriority(FilePriority priority)
    {
        return priority switch
        {
            FilePriority.Skip => MonoTorrent.Priority.DoNotDownload,
            FilePriority.Low => MonoTorrent.Priority.Low,
            FilePriority.Normal => MonoTorrent.Priority.Normal,
            FilePriority.High => MonoTorrent.Priority.High,
            _ => throw new ArgumentOutOfRangeException(nameof(priority), $"Invalid file priority: {priority}"),
        };
    }

    private Download WithProgress(Download state)
    {
        var files = fileMappings.Select(m => new DownloadFile(
            m.DestinationPath,
            m.Priority,
            MonoTorrent.ITorrentFileInfoExtensions.BytesDownloaded(m.SourceFile),
            m.SourceFile.Length,
            Percent(MonoTorrent.ITorrentFileInfoExtensions.BytesDownloaded(m.SourceFile), m.SourceFile.Length))).ToList();
        var targetFiles = torrentManager!.Files.Where(f => f.Priority != MonoTorrent.Priority.DoNotDownload).ToList();
        var downloadedBytes = targetFiles.Sum(MonoTorrent.ITorrentFileInfoExtensions.BytesDownloaded);
        var targetBytes = targetFiles.Sum(f => f.Length);
        var totalBytes = torrentManager.Files.Sum(f => f.Length);
        var partialProgressPercent = Percent(downloadedBytes, targetBytes);
        var targetProgressPercent = Percent(targetBytes, totalBytes);
        var totalProgressPercent = Percent(downloadedBytes, totalBytes);
        return state with
        {
            Files = files,
            DownloadedBytes = downloadedBytes,
            TargetBytes = targetBytes,
            TotalBytes = totalBytes,
            PartialProgressPercent = partialProgressPercent,
            TargetProgressPercent = targetProgressPercent,
            TotalProgressPercent = totalProgressPercent,
            BytesPerSecond = torrentManager.Monitor.DownloadRate,
            Seeds = torrentManager.Peers.Seeds,
        };

        double Percent(double numerator, double denominator)
        {
            return denominator == 0 ? 0 : 100.0 * numerator / denominator;
        }
    }

    private Task NotifyAdded() => eventStream.SendEventAsync(new DownloadAddedEvent(State));
    private Task NotifyProgress() => eventStream.SendEventAsync(new DownloadProgressEvent(State));
    private Task NotifyCompleted() => eventStream.SendEventAsync(new DownloadCompletedEvent(State));
    private Task NotifyRemoved() => eventStream.SendEventAsync(new DownloadRemoved(InfoHash));
    private Task NotifyFailed() => eventStream.SendEventAsync(new DownloadFailedEvent(State));

    private void ThrowIfDeleteRequested()
    {
        if (deleteRequested) throw new DeleteRequestedException();
    }

    private class DeleteRequestedException : Exception
    {
    }
}
