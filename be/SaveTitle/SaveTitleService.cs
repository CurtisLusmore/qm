using be.Interfaces;
using be.Models;

namespace be.SaveTitle;

public class SaveTitleService(ITitleSaver titleSaver)
{
    public async Task<Result<int>> SaveMovieAsync(Movie title, CancellationToken cancellationToken)
    {
        try
        {
            await titleSaver.SaveMovieAsync(title, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Failed to save movie: {ex.Message}");
        }
        return Result<int>.Success(0);
    }

    public async Task<Result<int>> SaveSeriesAsync(Series title, CancellationToken cancellationToken)
    {
        try
        {
            await titleSaver.SaveSeriesAsync(title, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Failed to save series: {ex.Message}");
        }
        return Result<int>.Success(0);
    }
}
