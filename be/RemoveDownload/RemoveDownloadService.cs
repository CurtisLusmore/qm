using be.Interfaces;
using be.Shared;

namespace be.RemoveDownload;

public class RemoveDownloadService(IDownloadRemover downloadRemover)
{
    public Result<int> RemoveDownload(string infoHash)
    {
        try
        {
            downloadRemover.RequestRemoveDownload(infoHash);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Failed to remove download: {ex.Message}");
        }
        return Result<int>.Success(0);
    }
}
