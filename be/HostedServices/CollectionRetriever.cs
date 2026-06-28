using System.Text.Json;
using be.Interfaces;
using be.Shared;

namespace be.HostedServices;

public partial class DownloadManagementService : ICollectionRetriever
{
    public async Task<Collection> GetCollectionAsync()
    {
        var movies = await GetMoviesAsync();
        var series = await GetSeriesAsync();
        return new Collection(
            movies,
            series);
    }

    private async Task<IEnumerable<Movie>> GetMoviesAsync()
    {
        var directory = moviesTitlesDirectory;
        var files = Directory.EnumerateFiles(directory, "*.json", SearchOption.TopDirectoryOnly);
        var movies = new List<Movie>();
        foreach (var file in files)
        {
            if (Path.GetFileName(file).StartsWith("._")) continue;
            try
            {
                var content = await File.ReadAllTextAsync(file);
                var movie = JsonSerializer.Deserialize<Movie>(content);
                if (movie == null) continue;
                var mediaFile = GetMovieMediaFile(movie.Id);
                if (mediaFile != null)
                {
                    movie = movie with { Downloaded = true };
                }
                movies.Add(movie);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to read movie file {File}: {Reason}", file, ex.Message);
            }
        }
        return movies;
    }

    private async Task<IEnumerable<Series>> GetSeriesAsync()
    {
        var directory = seriesTitlesDirectory;
        var files = Directory.EnumerateFiles(directory, "*.json", SearchOption.TopDirectoryOnly);
        var seriesList = new List<Series>();
        foreach (var file in files)
        {
            if (Path.GetFileName(file).StartsWith("._")) continue;
            try
            {
                var content = await File.ReadAllTextAsync(file);
                var series = JsonSerializer.Deserialize<Series>(content);
                if (series == null) continue;
                var episodes = new List<Episode>();
                foreach (var episode in series.Episodes)
                {
                    var newEpisode = episode;
                    var mediaFile = GetEpisodeMediaFile(series.Id, episode.SeasonNumber, episode.EpisodeNumber);
                    if (mediaFile != null)
                    {
                        newEpisode = episode with { Downloaded = true };
                    }
                    episodes.Add(newEpisode);
                }
                series = series with { Episodes = episodes.ToArray() };
                if (series.Episodes.All(e => e.Downloaded))
                {
                    series = series with { Downloaded = true };
                }
                seriesList.Add(series);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to read series file {File}: {Reason}", file, ex.Message);
            }
        }
        return seriesList;
    }
}
