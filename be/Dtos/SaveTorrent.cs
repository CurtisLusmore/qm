namespace qm.Dtos;

/// <summary>
/// The details of the torrent to save
/// </summary>
/// <param name="InfoHash">The info hash</param>
/// <param name="Name">The name</param>
public record SaveTorrent(
    string InfoHash,
    string Name);
