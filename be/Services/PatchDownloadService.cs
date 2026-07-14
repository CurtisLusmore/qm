using be.Models;
using be.Interfaces;

namespace be.Services;

public partial class DownloadManagementService : IDownloadPatcher
{
    public async Task RequestPatchDownloadAsync(string infoHash, DownloadPatch patch)
    {
        if (!activeInfoHashes.Contains(infoHash))
        {
            logger.LogWarning("Attempted to patch download {InfoHash} which is not active", infoHash);
            return;
        }

        var manager = managers.Values.FirstOrDefault(m => m.InfoHash == infoHash);
        if (manager == null)
        {
            logger.LogWarning("Attempted to patch download {InfoHash} but no manager was found", infoHash);
            return;
        }

        await manager.RequestPatchAsync(patch);

        logger.LogInformation("Requested patch of download {InfoHash}", infoHash);
    }
}