using be.Interfaces;

namespace be.HostedServices;

public partial class DownloadManagementService : ITitleRemover
{
    public async Task RemoveMovieAsync(string titleId)
    {
        try
        {
            var filename = MovieFile(titleId);
            if (File.Exists(filename))
            {
                File.Delete(filename);
                logger.LogInformation("Removed details for {TitleId}", titleId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to remove details for {TitleId}: {Reason}", titleId, ex.Message);
            throw;
        }
    }

    public async Task RemoveSeriesAsync(string titleId)
    {
        try
        {
            var filename = SeriesFile(titleId);
            if (File.Exists(filename))
            {
                File.Delete(filename);
                logger.LogInformation("Removed details for {TitleId}", titleId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to remove details for {TitleId}: {Reason}", titleId, ex.Message);
            throw;
        }
    }
}
