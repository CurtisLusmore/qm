using be.Shared;

namespace be.HostedServices;

public partial class DownloadManagementService
{
    private async Task DetectSavedDownloadsAsync(CancellationToken stoppingToken)
    {
        await foreach (var entry in keyValueStore.ListAsync<MovieOrSeries>("torrents/titles", cancellationToken: stoppingToken))
        {
            var (_, infoHash, title) = entry;
            if (trackers.ContainsKey(infoHash)) continue;
            logger.LogInformation("Loaded tracker for {InfoHash}: {Name}", infoHash, title.Name);
            trackers[infoHash] = new DownloadTracker(infoHash, title);
        }
    }
}
