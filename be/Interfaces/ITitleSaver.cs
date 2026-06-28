using be.Shared;

namespace be.Interfaces;

public interface ITitleSaver
{
    Task SaveMovieAsync(Movie title);
    Task SaveSeriesAsync(Series title);
}
