using be.Shared;
using System.Text.Json;

namespace be.HostedServices;

public partial class DownloadManagementService
{
    private async Task DetectSavedDownloadsAsync(CancellationToken cancellationToken)
    {
        var files = Directory.GetFiles(titlesDirectory, "*.json");
        foreach (var file in files)
        {
            var infoHash = Path.GetFileNameWithoutExtension(file);
            if (trackers.ContainsKey(infoHash) || infoHash.Length != 40) continue;
            MovieOrSeries title;
            try
            {
                title = JsonSerializer.Deserialize<MovieOrSeries>(await File.ReadAllTextAsync(file, cancellationToken));
            }
            catch (JsonException ex)
            {
                logger.LogWarning(ex, "Failed to load tracker for {InfoHash}: {File} is invalid JSON", infoHash, file);
                continue;
            }
            if (title is null)
            {
                logger.LogWarning("Failed to load tracker for {InfoHash}: file is empty or invalid", infoHash);
                continue;
            }
            logger.LogInformation("Loaded tracker for {InfoHash}: {Name}", infoHash, title.Name);
            trackers[infoHash] = new DownloadTracker(infoHash, title);
        }
    }
}
