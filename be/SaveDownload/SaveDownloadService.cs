using be.Interfaces;
using be.Shared;

namespace be.SaveDownload;

public class SaveDownloadService(IDownloadSaver downloadSaver)
{
    public async Task<Result<int>> DownloadAsync(SaveDownloadRequest request)
    {
        try
        {
            await downloadSaver.SaveDownloadAsync(request.InfoHash, request.Title);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Failed to save download: {ex.Message}");
        }
        return Result<int>.Success(0);
    }
}
