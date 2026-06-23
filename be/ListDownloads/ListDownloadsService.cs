using be.Interfaces;
using be.Shared;

namespace be.ListDownloads;

public class ListDownloadsService(IDownloadLister downloadLister)
{
    public async Task<Result<IEnumerable<DownloadTracker>>> ListDownloadsAsync()
    {
        try
        {
            var downloads = downloadLister.ToArray();
            return Result<IEnumerable<DownloadTracker>>.Success(downloads);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<DownloadTracker>>.Failure($"Failed to list downloads: {ex.Message}");
        }
    }
}
