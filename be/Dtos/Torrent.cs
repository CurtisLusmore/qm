namespace qm.Dtos;

/// <summary>
/// A saved torrent
/// </summary>
/// <param name="InfoHash">The info hash</param>
/// <param name="Name">The name</param>
/// <param name="State">The state</param>
/// <param name="Seeders">The number of seeders</param>
/// <param name="DownloadedBytes">The amount downloaded in bytes of all requested files</param>
/// <param name="TargetBytes">The total size in bytes of all requested files</param>
/// <param name="SizeBytes">The total size in bytes of all files</param>
/// <param name="PartialProgressPercent">The percentage of target files downloaded (from 0 to 100)</param>
/// <param name="TargetPercent">The percentage of all files that are targetted for download (from 0 to 100)</param>
/// <param name="ProgressPercent">The percentage of all files downloaded (from 0 to 100)</param>
/// <param name="BytesPerSecond">The download speed</param>
/// <param name="Files">The list of files</param>
public record Torrent(
    string InfoHash,
    string Name,
    State State,
    int Seeders,
    long DownloadedBytes,
    long TargetBytes,
    long SizeBytes,
    decimal PartialProgressPercent,
    decimal TargetPercent,
    decimal ProgressPercent,
    double BytesPerSecond,
    TorrentFile[] Files)
{
}
