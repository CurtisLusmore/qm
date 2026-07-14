using be.Interfaces;
using be.Models;

namespace be.Services;

public partial class DownloadManagementService : ITitleLister
{
    public IAsyncEnumerable<Movie> ListMoviesAsync(DateTime addedSince, CancellationToken cancellationToken)
    {
        return fileService.ListMoviesAsync(addedSince, cancellationToken);
    }

    public IAsyncEnumerable<Series> ListSeriesAsync(DateTime addedSince, CancellationToken cancellationToken)
    {
        return fileService.ListSeriesAsync(addedSince, cancellationToken);
    }
}
