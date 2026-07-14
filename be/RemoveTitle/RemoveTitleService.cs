using System.Net;
using be.Interfaces;
using be.Models;

namespace be.RemoveTitle;

public class RemoveTitleService(ITitleRemover titleRemover)
{
    public async Task<Result> RemoveMovieAsync(string titleId)
    {
        try
        {
            await titleRemover.RemoveMovieAsync(titleId);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to remove movie: {ex.Message}");
        }
        return Result.Success(HttpStatusCode.Accepted);
    }

    public async Task<Result> RemoveSeriesAsync(string titleId)
    {
        try
        {
            await titleRemover.RemoveSeriesAsync(titleId);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to remove series: {ex.Message}");
        }
        return Result.Success(HttpStatusCode.Accepted);
    }
}
