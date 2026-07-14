using System.Net;
using be.Interfaces;
using be.Models;

namespace be.RemoveDownload;

public class RemoveDownloadService(IDownloadRemover downloadRemover)
{
    public async Task<Result> RemoveDownloadAsync(string infoHash)
    {
        try
        {
            await downloadRemover.RequestRemoveDownloadAsync(infoHash);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to remove download: {ex.Message}");
        }
        return Result.Success(HttpStatusCode.Accepted);
    }
}
