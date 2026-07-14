using be.Models;

namespace be.Interfaces;

public interface ITitleLister
{
    IAsyncEnumerable<Movie> ListMoviesAsync(DateTime addedSince, CancellationToken cancellationToken);
    IAsyncEnumerable<Series> ListSeriesAsync(DateTime addedSince, CancellationToken cancellationToken);
}
