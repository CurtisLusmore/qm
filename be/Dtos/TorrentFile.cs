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
    Priority Priority);
