using System.Net;
using be.Interfaces;
using be.Models;

namespace be.SaveDownload;

public class SaveDownloadService(IDownloadSaver downloadSaver)
{
    public async Task<Result> DownloadAsync(SaveDownloadRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await downloadSaver.SaveDownloadAsync(request.InfoHash, request.Title, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to save download: {ex.Message}");
        }
        return Result.Success(HttpStatusCode.Accepted);
    }
}
