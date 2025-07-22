namespace qm.Dtos;

/// <summary>
/// A saved torrent
/// </summary>
/// <param name="InfoHash">The info hash</param>
/// <param name="Name">The name</param>
/// <param name="State">The state</param>
/// <param name="Seeders">The number of seeders</param>
/// <param name="DownloadedBytes">The amount downloaded in bytes</param>
/// <param name="SizeBytes">The size in bytes</param>
/// <param name="ProgressPercent">The percentage downloaded (from 0 to 100)</param>
/// <param name="NumFiles">The number of files</param>
/// <param name="Files">The list of files</param>
public record Torrent(
    string InfoHash,
    string Name,
    State State,
    int Seeders,
    long DownloadedBytes,
    long SizeBytes,
    decimal ProgressPercent,
    int NumFiles,
    TorrentFile[] Files)
{
    /// <summary>
    /// A saved torrent
    /// </summary>
    /// <param name="InfoHash">The info hash</param>
    /// <param name="Name">The name</param>
    /// <param name="State">The state</param>
    /// <param name="Seeders">The number of seeders</param>
    /// <param name="DownloadedBytes">The amount downloaded in bytes</param>
    /// <param name="SizeBytes">The size in bytes</param>
    /// <param name="NumFiles">The number of files</param>
    /// <param name="Files">The list of files</param>
    public Torrent(
        string InfoHash,
        string Name,
        State State,
        int Seeders,
        long DownloadedBytes,
        long SizeBytes,
        int NumFiles,
        TorrentFile[] Files) : this(
            InfoHash,
            Name,
            State,
            Seeders,
            DownloadedBytes,
            SizeBytes,
            SizeBytes == 0 ? 0 : 100.0m * DownloadedBytes / SizeBytes,
            NumFiles,
            Files)
    {
    }
}
