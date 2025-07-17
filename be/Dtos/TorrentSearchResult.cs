namespace qm.Dtos;

/// <summary>
/// A torrent search result
/// </summary>
/// <param name="InfoHash">The info hash</param>
/// <param name="Name">The name</param>
/// <param name="Seeders">The number of seeders</param>
/// <param name="SizeBytes">THe size in bytes</param>
/// <param name="NumFiles">The number of files</param>
public record TorrentSearchResult(
    string InfoHash,
    string Name,
    int Seeders,
    long SizeBytes,
    int NumFiles);
