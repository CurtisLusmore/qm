namespace qm.Dtos;

/// <summary>
/// An update to the priority of a torrent file
/// </summary>
/// <param name="Path">The path</param>
/// <param name="Priority">The new priority</param>
public record TorrentFilePatch(
    string Path,
    Priority Priority);
