using MonoTorrent;

namespace qm.Dtos;

/// <summary>
/// A file of a saved torrent
/// </summary>
/// <param name="Path">The path</param>
/// <param name="DownloadedBytes">The amount downloaded in bytes</param>
/// <param name="SizeBytes">The size in bytes</param>
/// <param name="ProgressPercent">The percentage downloaded (from 0 to 100)</param>
/// <param name="Priority">The priority</param>
public record TorrentFile(
    string Path,
    long DownloadedBytes,
    long SizeBytes,
    decimal ProgressPercent,
    Priority Priority)
{
    /// <summary>
    /// Convert from a <see cref="ITorrentManagerFile"/>
    /// </summary>
    /// <param name="file">The file</param>
    /// <returns>The converted file</returns>
    public static TorrentFile ConvertFrom(ITorrentManagerFile file)
    {
        var downloadedBytes = ITorrentFileInfoExtensions.BytesDownloaded(file);
        var sizeBytes = file.Length;
        var progressPercent = sizeBytes == 0 ? 0 : 100.0m * downloadedBytes / sizeBytes;
        return new TorrentFile(
            file.Path,
            downloadedBytes,
            sizeBytes,
            progressPercent,
            Convert(file.Priority));
    }

    private static Priority Convert(MonoTorrent.Priority priority) => priority switch
    {
        MonoTorrent.Priority.DoNotDownload => Priority.Skip,
        MonoTorrent.Priority.Lowest => Priority.Normal,
        MonoTorrent.Priority.Low => Priority.Normal,
        MonoTorrent.Priority.Normal => Priority.Normal,
        MonoTorrent.Priority.High => Priority.High,
        MonoTorrent.Priority.Highest => Priority.High,
        MonoTorrent.Priority.Immediate => Priority.High,
        _ => throw new ArgumentOutOfRangeException(nameof(priority)),
    };
}
