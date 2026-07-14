using be.Interfaces;
using be.Models;

namespace be.Services;

public partial class DownloadManagementService : ITitleSaver
{
    public async Task SaveMovieAsync(Movie title, CancellationToken cancellationToken)
    {
        await fileService.SaveMovieAsync(title, cancellationToken);
        logger.LogInformation("Saved movie with titleId: {TitleId}", title.Id);
    }

    public async Task SaveSeriesAsync(Series title, CancellationToken cancellationToken)
    {
        await fileService.SaveSeriesAsync(title, cancellationToken);
        logger.LogInformation("Saved series with titleId: {TitleId}", title.Id);
    }
}
