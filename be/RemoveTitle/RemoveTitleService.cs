using be.Interfaces;
using be.Shared;

namespace be.RemoveTitle;

public class RemoveTitleService(ITitleRemover titleRemover)
{
    public async Task<Result<int>> RemoveMovieAsync(string titleId)
    {
        try
        {
            await titleRemover.RemoveMovieAsync(titleId);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Failed to remove movie: {ex.Message}");
        }
        return Result<int>.Success(0);
    }

    public async Task<Result<int>> RemoveSeriesAsync(string titleId)
    {
        try
        {
            await titleRemover.RemoveSeriesAsync(titleId);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Failed to remove series: {ex.Message}");
        }
        return Result<int>.Success(0);
    }
}
