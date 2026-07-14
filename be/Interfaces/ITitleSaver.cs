using be.Models;

namespace be.Interfaces;

public interface ITitleSaver
{
    Task SaveMovieAsync(Movie title, CancellationToken cancellationToken);
    Task SaveSeriesAsync(Series title, CancellationToken cancellationToken);
}
