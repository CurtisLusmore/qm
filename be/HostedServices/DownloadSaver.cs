using be.Interfaces;
using be.Shared;
using System.Text.Json;

namespace be.HostedServices;

public partial class DownloadManagementService : IDownloadSaver
{
    public async Task SaveDownloadAsync(string infoHash, MovieOrSeries title)
    {
        infoHash = infoHash.ToUpperInvariant();
        try
        {
            var filename = TitleFile(infoHash);
            using (var writer = File.CreateText($"{filename}.tmp"))
            {
                await writer.WriteAsync(JsonSerializer.Serialize(title));
            }
            File.Move($"{filename}.tmp", filename, true);
            logger.LogInformation("Saved tracker for {InfoHash}: {Name}", infoHash, title.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save tracker for {InfoHash}: {Reason}", infoHash, ex.Message);
            throw;
        }
    }
}
