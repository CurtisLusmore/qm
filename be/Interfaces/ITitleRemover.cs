namespace be.Interfaces;

public interface ITitleRemover
{
    Task RemoveMovieAsync(string titleId);
    Task RemoveSeriesAsync(string titleId);
}
