using be.Config;
using be.Models;
using Microsoft.Extensions.Options;

namespace be.Clients;

public class DownloadFileClient(IOptions<LibraryConfig> config)
{
    public string DownloadingDirectory(string infoHash, MovieOrSeries title)
        => Path.Combine(config.Value.RootDirectory, ".torrents", "downloading", $"{title.Name} ({title.Year}) [{title.Id}] [{infoHash}]");
    public string BinDirectory(string infoHash, MovieOrSeries title)
        => Path.Combine(config.Value.RootDirectory, ".torrents", "bin", $"{title.Name} ({title.Year}) [{title.Id}] [{infoHash}]");

    public void DeleteDownloadingFiles(string infoHash, MovieOrSeries title)
    {
        var sourceDirectory = DownloadingDirectory(infoHash, title);
        var binDirectory = BinDirectory(infoHash, title);
        if (Directory.Exists(sourceDirectory))
        {
            DeleteDirectory(binDirectory);
            Directory.Move(sourceDirectory, binDirectory);
        }
    }

    public void MoveCompletedFiles(string infoHash, MovieOrSeries title, IEnumerable<FileMapping> fileMappings)
    {
        var sourceDirectory = DownloadingDirectory(infoHash, title);
        var destinationDirectory = FinalDirectory(title);
        EnsureDirectoryExists(destinationDirectory);
        foreach (var (_, filename, priority, _) in fileMappings)
        {
            var sourcePath = Path.Combine(sourceDirectory, filename);
            if (priority == FilePriority.Skip) continue;
            var destinationFilePath = Path.Combine(destinationDirectory, filename);
            File.Move(sourcePath, destinationFilePath, overwrite: true);
        }
        var binDirectory = BinDirectory(infoHash, title);
        DeleteDirectory(binDirectory);
        Directory.Move(sourceDirectory, binDirectory);
    }

    public MediaFile? GetMovieMediaFile(string titleId)
    {
        var directory = Path.Combine(config.Value.RootDirectory, "Movies");
        var files = Directory
            .GetFiles(directory, $"* [{titleId}].*", SearchOption.AllDirectories)
            .Where(IsVideoFile);

        return files.SingleOrDefault() is string filePath
            ? new MediaFile(titleId, filePath, GetMimeType(filePath))
            : null;
    }

    public MediaFile? GetEpisodeMediaFile(string titleId, int seasonNumber, int episodeNumber)
    {
        var directory = Path.Combine(config.Value.RootDirectory, "Series");
        var seriesDirectory = Directory
            .GetDirectories(directory, $"* [{titleId}]", SearchOption.AllDirectories)
            .SingleOrDefault();
        if (seriesDirectory is null) return null;
        var files = Directory
            .GetFiles(seriesDirectory, $"S{seasonNumber:D2}E{episodeNumber:D2}*.*", SearchOption.AllDirectories)
            .Where(IsVideoFile);

        return files.SingleOrDefault() is string filePath
            ? new MediaFile(titleId, filePath, GetMimeType(filePath), seasonNumber, episodeNumber)
            : null;
    }

    private string FinalDirectory(TitleSummary title)
        => title.Type switch
        {
            TitleType.Movie => Path.Combine(config.Value.RootDirectory, "Movies"),
            TitleType.Series => Path.Combine(config.Value.RootDirectory, "Series", $"{title.Name} ({title.Year}) [{title.Id}]"),
            _ => throw new ArgumentOutOfRangeException()
        };

    private void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }

    private void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    private static bool IsVideoFile(string filePath)
    {
        var videoExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm"
        };

        return videoExtensions.Contains(Path.GetExtension(filePath));
    }

    private static string GetMimeType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".mp4" => "video/mp4",
            ".mkv" => "video/x-matroska",
            ".avi" => "video/x-msvideo",
            ".mov" => "video/quicktime",
            ".wmv" => "video/x-ms-wmv",
            ".flv" => "video/x-flv",
            ".webm" => "video/webm",
            _ => "application/octet-stream"
        };
    }
}
