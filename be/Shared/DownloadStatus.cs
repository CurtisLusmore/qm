using System.Text.Json.Serialization;

namespace be.Shared;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DownloadStatus
{
    Received,
    DownloadingTorrentFile,
    DownloadTorrentFileFailed,
    DownloadedTorrentFile,
    AddedTorrent,
    StartedTorrent,
    InitializingTorrent,
    DownloadingTorrent,
    PausedTorrent,
    DownloadTorrentFailed,
    StoppingTorrent,
    DownloadedTorrent,
    SortingFiles,
    ManualSortingRequired,
    Completed
}
