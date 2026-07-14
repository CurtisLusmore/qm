using be.Config;
using be.PubSub;
using Microsoft.Extensions.Options;
using MonoTorrent.Client;

namespace be.Services;

public partial class DownloadManagementService(
    IOptions<LibraryConfig> config,
    FileService fileService,
    ILogger<DownloadManagementService> logger,
    ILoggerFactory loggerFactory) : BackgroundService
{
    private readonly ClientEngine torrentEngine = new(
        new EngineSettingsBuilder
        {
            CacheDirectory = Path.Combine(config.Value.RootDirectory, ".torrents"),
        }.ToSettings());
    private readonly Queue<DownloadManager> downloadQueue = new();
    private readonly Dictionary<Task, DownloadManager> managers = new();
    private readonly HashSet<string> activeInfoHashes = new();
    private readonly EventStream eventStream = new();

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await AddFromFileSystemAsync(cancellationToken);
            await AddFromQueueAsync(cancellationToken);
            await MonitorForCompletedAsync(cancellationToken);
        }
    }

    private async Task AddFromQueueAsync(CancellationToken cancellationToken)
    {
        while (downloadQueue.TryDequeue(out var manager))
        {
            managers[manager.RunAsync(cancellationToken)] = manager;
            logger.LogInformation("Started download manager for {Name}", manager.Name);
        }
    }

    private async Task AddFromFileSystemAsync(CancellationToken cancellationToken)
    {
        var infoHashes = fileService.ListInfoHashes();
        foreach (var infoHash in infoHashes)
        {
            if (activeInfoHashes.Contains(infoHash)) continue;

            activeInfoHashes.Add(infoHash);
            var manager = await DownloadManager.LoadAsync(
                infoHash,
                fileService,
                torrentEngine,
                this,
                loggerFactory.CreateLogger<DownloadManager>(),
                cancellationToken);
            downloadQueue.Enqueue(manager);
            logger.LogInformation("Loaded download {InfoHash} {Name}", infoHash, manager.Name);
        }
    }

    private async Task MonitorForCompletedAsync(CancellationToken cancellationToken)
    {
        var delayTask = Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        var completed = await Task.WhenAny([ delayTask, ..managers.Keys ]);
        if (completed == delayTask) return;

        var manager = managers[completed];
        managers.Remove(completed);
        try
        {
            await completed;
            activeInfoHashes.Remove(manager.InfoHash);
            logger.LogInformation("Download manager for {Name} completed successfully", manager.Name);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error in download manager for {Name}: {Message}", manager.Name, ex.Message);
            logger.LogInformation("Restarting download for {Name}", manager.Name);
            managers[manager.RunAsync(cancellationToken)] = manager;
        }
    }
}
