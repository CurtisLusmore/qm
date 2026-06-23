using be.Interfaces;
using be.Shared;

namespace be.HostedServices;

public partial class DownloadManagementService : IDownloadSaver
{
    public async Task SaveDownloadAsync(string infoHash, MovieOrSeries title)
    {
        infoHash = infoHash.ToUpperInvariant();
        try
        {
            await keyValueStore.PutAsync("torrents/titles", infoHash, title);
            logger.LogInformation("Saved tracker for {InfoHash}: {Name}", infoHash, title.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save tracker for {InfoHash}: {Reason}", infoHash, ex.Message);
            throw;
        }
    }
}
