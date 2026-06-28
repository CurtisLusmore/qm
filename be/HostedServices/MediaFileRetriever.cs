using be.Interfaces;
using be.Shared;

namespace be.HostedServices;

public partial class DownloadManagementService : IMediaFileRetriever
{
    public MediaFile? GetMovieMediaFile(string titleId)
    {
        var files = Directory.GetFiles(moviesDirectory, $"*[{titleId}].*", SearchOption.TopDirectoryOnly);
        return files.FirstOrDefault(file => videoExtensions.Contains(Path.GetExtension(file))) is string filePath
            ? new MediaFile(
                titleId,
                filePath,
                GetMimeType(filePath))
            : null;
    }

    public MediaFile? GetEpisodeMediaFile(string titleId, int seasonNumber, int episodeNumber)
    {
        var directories = Directory.GetDirectories(seriesDirectory, $"*[{titleId}]", SearchOption.TopDirectoryOnly);
        var directory = directories.FirstOrDefault();
        if (directory == null) return null;
        var files = Directory.GetFiles(directory, $"*S{seasonNumber:D2}E{episodeNumber:D2}*.*", SearchOption.TopDirectoryOnly);
        return files.FirstOrDefault(file => videoExtensions.Contains(Path.GetExtension(file))) is string filePath
            ? new MediaFile(
                titleId,
                filePath,
                GetMimeType(filePath),
                seasonNumber,
                episodeNumber)
            : null;
    }

    private static readonly string[] videoExtensions = [ ".mp4", ".mkv", ".avi", ".mov", ".wmv" ];

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
            _ => "application/octet-stream"
        };
    }
}
