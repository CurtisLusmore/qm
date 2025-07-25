using MonoTorrent.Client;

namespace qm.Dtos;

/// <summary>
/// A saved torrent
/// </summary>
/// <param name="InfoHash">The info hash</param>
/// <param name="Name">The name</param>
/// <param name="State">The state</param>
/// <param name="Seeders">The number of seeders</param>
/// <param name="DownloadedBytes">The amount downloaded in bytes</param>
/// <param name="TargetBytes">The total size in bytes of all requested files</param>
/// <param name="SizeBytes">The total size in bytes of all files</param>
/// <param name="PartialProgressPercent">The percentage of target files downloaded (from 0 to 100)</param>
/// <param name="TargetPercent">The percentage of all files that are targetted for download (from 0 to 100)</param>
/// <param name="ProgressPercent">The percentage downloaded (from 0 to 100)</param>
/// <param name="NumFiles">The number of files</param>
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
    int NumFiles,
    TorrentFile[] Files)
{
    /// <summary>
    /// Convert from a <see cref="TorrentManager"/>
    /// </summary>
    /// <param name="infoHash">The info hash, in case it hasn't loaded yet</param>
    /// <param name="torrent">The torrent</param>
    /// <returns>The converted torrent</returns>
    public static Torrent ConvertFrom(string infoHash, TorrentManager torrent)
    {
        var files = torrent.Files
            .Select(TorrentFile.ConvertFrom)
            .OrderBy(file => file.Path)
            .ToArray();
        var downloadingFiles = files.Where(file => file.Priority != Priority.Skip);
        var partialDownloadedBytes = downloadingFiles.Sum(file => file.DownloadedBytes);
        var downloadedBytes = files.Sum(file => file.DownloadedBytes);
        var targetBytes = downloadingFiles.Sum(file => file.SizeBytes);
        var sizeBytes = files.Sum(file => file.SizeBytes);
        var partialProgressPercent = targetBytes == 0 ? 0 : 100.0m * partialDownloadedBytes / targetBytes;
        var targetPercent = sizeBytes == 0 ? 0 : 100.0m * targetBytes / sizeBytes;
        var progressPercent = sizeBytes == 0 ? 0 : 100.0m * downloadedBytes / sizeBytes;
        return new Torrent(
            infoHash,
            torrent.Torrent?.Name ?? "Loading...",
            Convert(torrent.State),
            torrent.Peers.Seeds,
            downloadedBytes,
            targetBytes,
            sizeBytes,
            partialProgressPercent,
            targetPercent,
            progressPercent,
            files.Length,
            files);
    }

    private static State Convert(TorrentState state) => state switch
    {
        TorrentState.Stopped => State.Complete,
        TorrentState.Paused => State.Paused,
        TorrentState.Starting => State.Initializing,
        TorrentState.Downloading => State.Downloading,
        TorrentState.Seeding => State.Complete,
        TorrentState.Hashing => State.Initializing,
        TorrentState.HashingPaused => State.Paused,
        TorrentState.Stopping => State.Downloading,
        TorrentState.Error => State.Error,
        TorrentState.Metadata => State.Initializing,
        TorrentState.FetchingHashes => State.Initializing,
        _ => throw new ArgumentOutOfRangeException(nameof(state)),
    };
}
