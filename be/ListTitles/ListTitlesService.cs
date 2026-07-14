using be.Interfaces;
using be.Models;

namespace be.ListTitles;

public class ListTitlesService(ITitleLister titleLister)
{
    public IAsyncEnumerable<Movie> ListMoviesAsync(DateTime addedSince, CancellationToken cancellationToken)
    {
        return titleLister.ListMoviesAsync(addedSince, cancellationToken);
    }

    public IAsyncEnumerable<Series> ListSeriesAsync(DateTime addedSince, CancellationToken cancellationToken)
    {
        return titleLister.ListSeriesAsync(addedSince, cancellationToken);
    }
}
