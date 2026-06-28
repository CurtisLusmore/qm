using be.Interfaces;
using be.Shared;

namespace be.SaveTitle;

public class SaveTitleService(ITitleSaver titleSaver)
{
    public async Task<Result<int>> SaveMovieAsync(Movie title)
    {
        try
        {
            await titleSaver.SaveMovieAsync(title);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Failed to save movie: {ex.Message}");
        }
        return Result<int>.Success(0);
    }

    public async Task<Result<int>> SaveSeriesAsync(Series title)
    {
        try
        {
            await titleSaver.SaveSeriesAsync(title);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Failed to save series: {ex.Message}");
        }
        return Result<int>.Success(0);
    }
}
