using System.Net;
using be.Interfaces;
using be.Models;

namespace be.PatchDownload;

public class PatchDownloadService(IDownloadPatcher downloadPatcher)
{
    public async Task<Result> PatchDownloadAsync(string infoHash, DownloadPatch patch)
    {
        try
        {
            await downloadPatcher.RequestPatchDownloadAsync(infoHash, patch);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to patch download: {ex.Message}");
        }
        return Result.Success(HttpStatusCode.Accepted);
    }
}
