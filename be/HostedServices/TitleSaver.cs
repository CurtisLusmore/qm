using be.Interfaces;
using be.Shared;
using System.Text.Json;

namespace be.HostedServices;

public partial class DownloadManagementService : ITitleSaver
{
    public async Task SaveMovieAsync(Movie title)
    {
        try
        {
            var filename = MovieFile(title.Id);
            using var writer = File.CreateText($"{filename}.tmp");
            await writer.WriteAsync(JsonSerializer.Serialize(title));
            File.Move($"{filename}.tmp", filename, true);
            logger.LogInformation("Saved details for {TitleId}: {Name}", title.Id, title.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save details for {TitleId}: {Reason}", title.Id, ex.Message);
            throw;
        }
    }

    public async Task SaveSeriesAsync(Series title)
    {
        try
        {
            var filename = SeriesFile(title.Id);
            using var writer = File.CreateText($"{filename}.tmp");
            await writer.WriteAsync(JsonSerializer.Serialize(title));
            File.Move($"{filename}.tmp", filename, true);
            logger.LogInformation("Saved details for {TitleId}: {Name}", title.Id, title.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save details for {TitleId}: {Reason}", title.Id, ex.Message);
            throw;
        }
    }
}
