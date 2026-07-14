namespace be.Models;

public record FileMapping(
    MonoTorrent.ITorrentManagerFile SourceFile,
    string DestinationPath,
    FilePriority Priority,
    EpisodeInfo? EpisodeInfo = null);
