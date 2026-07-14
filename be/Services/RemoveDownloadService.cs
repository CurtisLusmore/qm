using be.Interfaces;

namespace be.Services;

public partial class DownloadManagementService : IDownloadRemover
{
    public async Task RequestRemoveDownloadAsync(string infoHash)
    {
        if (!activeInfoHashes.Contains(infoHash))
        {
            logger.LogWarning("Attempted to remove download {InfoHash} which is not active", infoHash);
            return;
        }

        var manager = managers.Values.FirstOrDefault(m => m.InfoHash == infoHash);
        if (manager == null)
        {
            logger.LogWarning("Attempted to remove download {InfoHash} but no manager was found", infoHash);
            return;
        }

        await manager.RequestDeleteAsync();
        logger.LogInformation("Requested removal of download {InfoHash}", infoHash);
    }
}