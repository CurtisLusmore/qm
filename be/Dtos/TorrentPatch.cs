namespace qm.Dtos;

/// <summary>
/// An update to the state of a torrent
/// </summary>
/// <param name="State">The new state</param>
/// <param name="Files">File patches</param>
public record TorrentPatch(
    PatchState? State,
    TorrentFilePatch[]? Files);
