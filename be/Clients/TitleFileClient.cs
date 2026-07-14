using be.Config;
using be.Models;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace be.Clients;

public class TitleFileClient(IOptions<LibraryConfig> config)
{
    public IEnumerable<string> ListInfoHashes()
    {
        EnsureDirectoryExists();
        return Directory.EnumerateFiles(titlesDirectory, "*.json")
            .Where(f => !Path.GetFileName(f).StartsWith("._")) // Skip macOS resource fork files
            .Select(Path.GetFileNameWithoutExtension)
            .Select(name => name!.ToUpperInvariant());
    }

    public async Task<MovieOrSeries> LoadTitleFileAsync(string infoHash, CancellationToken cancellationToken)
    {
        EnsureDirectoryExists();
        var titleFile = TitleFilePath(infoHash);
        if (!File.Exists(titleFile))
        {
            throw new FileNotFoundException($"Title file not found: {titleFile}");
        }

        using var stream = File.OpenRead(titleFile);
        MovieOrSeries? title;
        try
        {
            title = await JsonSerializer.DeserializeAsync<MovieOrSeries>(stream, cancellationToken: cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new IOException($"Failed to read title file: {titleFile}", ex);
        }

        if (title == null)
        {
            throw new IOException($"Failed to read title file: {titleFile}");
        }

        return title;
    }

    public async Task SaveTitleFileAsync(string infoHash, MovieOrSeries title, CancellationToken cancellationToken)
    {
        var titleFile = TitleFilePath(infoHash);
        await SaveFileAsync(titleFile, title, cancellationToken);
    }

    public void DeleteTitleFile(string infoHash)
    {
        EnsureDirectoryExists();
        var titleFile = TitleFilePath(infoHash);
        if (File.Exists(titleFile))
        {
            try
            {
                File.Delete(titleFile);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new IOException($"Failed to delete title file: {titleFile}", ex);
            }
        }
    }

    public async Task SaveMovieAsync(Movie title, CancellationToken cancellationToken)
    {
        var titleFile = TitleFilePath(title);
        if (title.AddedOn == null) title = title with { AddedOn = DateTime.UtcNow };
        await SaveFileAsync(titleFile, title, cancellationToken);
    }

    public async Task SaveSeriesAsync(Series title, CancellationToken cancellationToken)
    {
        var titleFile = TitleFilePath(title);
        if (title.AddedOn == null) title = title with { AddedOn = DateTime.UtcNow };
        await SaveFileAsync(titleFile, title, cancellationToken);
    }

    public async Task<Movie> GetMovieAsync(string id, CancellationToken cancellationToken)
    {
        EnsureDirectoryExists();
        var titleFile = Path.Combine(moviesDirectory, $"{id}.json");
        if (!File.Exists(titleFile))
        {
            throw new FileNotFoundException($"Movie file not found: {titleFile}");
        }

        using var stream = File.OpenRead(titleFile);
        Movie? movie;
        try
        {
            movie = await JsonSerializer.DeserializeAsync<Movie>(stream, cancellationToken: cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new IOException($"Failed to read movie file: {titleFile}", ex);
        }

        if (movie == null)
        {
            throw new IOException($"Failed to read movie file: {titleFile}");
        }

        return movie;
    }

    public async IAsyncEnumerable<Movie> ListMoviesAsync(DateTime addedSince, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        EnsureDirectoryExists();
        var movieFiles = Directory.EnumerateFiles(moviesDirectory, "*.json").Where(f => !Path.GetFileName(f).StartsWith("._")); // Skip macOS resource fork files
        foreach (var movieFile in movieFiles)
        {
            if (Path.GetFileName(movieFile).StartsWith("._")) continue; // Skip macOS resource fork files
            if (File.GetLastWriteTimeUtc(movieFile) < addedSince) continue;
            using var stream = File.OpenRead(movieFile);
            Movie? movie;
            try
            {
                movie = await JsonSerializer.DeserializeAsync<Movie>(stream, cancellationToken: cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new IOException($"Failed to read movie file: {movieFile}", ex);
            }

            if (movie is not null) yield return movie;
        }
    }

    public async Task<Series> GetSeriesAsync(string id, CancellationToken cancellationToken)
    {
        EnsureDirectoryExists();
        var titleFile = Path.Combine(seriesDirectory, $"{id}.json");
        if (!File.Exists(titleFile))
        {
            throw new FileNotFoundException($"Series file not found: {titleFile}");
        }

        using var stream = File.OpenRead(titleFile);
        Series? series;
        try
        {
            series = await JsonSerializer.DeserializeAsync<Series>(stream, cancellationToken: cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new IOException($"Failed to read series file: {titleFile}", ex);
        }

        if (series == null)
        {
            throw new IOException($"Failed to read series file: {titleFile}");
        }

        return series;
    }

    public async IAsyncEnumerable<Series> ListSeriesAsync(DateTime addedSince, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        EnsureDirectoryExists();
        var seriesFiles = Directory.EnumerateFiles(seriesDirectory, "*.json").Where(f => !Path.GetFileName(f).StartsWith("._")); // Skip macOS resource fork files
        foreach (var seriesFile in seriesFiles)
        {
            if (Path.GetFileName(seriesFile).StartsWith("._")) continue; // Skip macOS resource fork files
            if (File.GetLastWriteTimeUtc(seriesFile) < addedSince) continue;
            using var stream = File.OpenRead(seriesFile);
            Series? series;
            try
            {
                series = await JsonSerializer.DeserializeAsync<Series>(stream, cancellationToken: cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new IOException($"Failed to read series file: {seriesFile}", ex);
            }

            if (series is not null) yield return series;
        }
    }

    public async Task MarkMovieDownloadedAsync(TitleSummary title, CancellationToken cancellationToken)
    {
        var movie = await GetMovieAsync(title.Id, cancellationToken);
        movie = movie with { Downloaded = true };
        await SaveMovieAsync(movie, cancellationToken);
    }

    public async Task MarkEpisodesDownloadedAsync(TitleSummary title, IEnumerable<EpisodeInfo> episodes, CancellationToken cancellationToken)
    {
        var series = await GetSeriesAsync(title.Id, cancellationToken);
        var updatedEpisodes = series.Episodes.Select(e =>
            episodes.Any(ep => ep.SeasonNumber == e.SeasonNumber && ep.EpisodeNumber == e.EpisodeNumber)
                ? e with { Downloaded = true }
                : e).ToArray();
        series = series with { Episodes = updatedEpisodes };
        if (series.Episodes.All(e => e.Downloaded)) series = series with { Downloaded = true };
        await SaveSeriesAsync(series, cancellationToken);
    }

    public async Task RemoveMovieAsync(string titleId)
    {
        var titleFile = Path.Combine(moviesDirectory, $"{titleId}.json");
        if (File.Exists(titleFile))
        {
            File.Delete(titleFile);
        }
    }

    public async Task RemoveSeriesAsync(string titleId)
    {
        var titleFile = Path.Combine(seriesDirectory, $"{titleId}.json");
        if (File.Exists(titleFile))
        {
            File.Delete(titleFile);
        }
    }

    private async Task SaveFileAsync<T>(string filename, T value, CancellationToken cancellationToken)
    {
        EnsureDirectoryExists();
        var tmpFile = $"{filename}.tmp";
        try
        {
            using (var stream = File.Create(tmpFile))
            {
                await JsonSerializer.SerializeAsync(stream, value, cancellationToken: cancellationToken);
            }
            File.Move(tmpFile, filename, overwrite: true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new IOException($"Failed to save file: {filename}", ex);
        }
    }

    private readonly string titlesDirectory = Path.Combine(config.Value.RootDirectory, ".torrents", "titles");
    private string TitleFilePath(string infoHash) => Path.Combine(titlesDirectory, $"{infoHash.ToUpperInvariant()}.json");
    private readonly string moviesDirectory = Path.Combine(config.Value.RootDirectory, "Movies", ".titles");
    private string TitleFilePath(Movie title) => Path.Combine(moviesDirectory, $"{title.Id}.json");
    private readonly string seriesDirectory = Path.Combine(config.Value.RootDirectory, "Series", ".titles");
    private string TitleFilePath(Series title) => Path.Combine(seriesDirectory, $"{title.Id}.json");

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(titlesDirectory))
        {
            Directory.CreateDirectory(titlesDirectory);
        }
        if (!Directory.Exists(moviesDirectory))
        {
            Directory.CreateDirectory(moviesDirectory);
        }
        if (!Directory.Exists(seriesDirectory))
        {
            Directory.CreateDirectory(seriesDirectory);
        }
    }
}