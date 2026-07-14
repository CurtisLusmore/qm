using be.Interfaces;
using be.Models;

namespace be.Services;

public partial class DownloadManagementService : IDownloadSaver
{
    public async Task SaveDownloadAsync(string infoHash, MovieOrSeries title, CancellationToken cancellationToken)
    {
        activeInfoHashes.Add(infoHash);
        var manager = await DownloadManager.AddAsync(
            infoHash,
            title,
            fileService,
            torrentEngine,
            this,
            loggerFactory.CreateLogger<DownloadManager>(),
            cancellationToken);
        downloadQueue.Enqueue(manager);
        logger.LogInformation("Saved download {InfoHash} {Name}", infoHash, manager.Name);
    }
}
