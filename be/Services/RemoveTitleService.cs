using be.Interfaces;
using be.Models;

namespace be.Services;

public partial class DownloadManagementService : ITitleRemover
{
    public async Task RemoveMovieAsync(string titleId)
    {
        await fileService.RemoveMovieAsync(titleId);
        logger.LogInformation("Removed movie with titleId: {TitleId}", titleId);
    }

    public async Task RemoveSeriesAsync(string titleId)
    {
        await fileService.RemoveSeriesAsync(titleId);
        logger.LogInformation("Removed series with titleId: {TitleId}", titleId);
    }
}
