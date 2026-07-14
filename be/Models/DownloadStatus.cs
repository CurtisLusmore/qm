using System.Text.Json.Serialization;

namespace be.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DownloadStatus
{
    Received,
    DownloadingTorrentFile,
    AddingTorrent,
    MappingFiles,
    LoadingFastResume,
    StartingTorrent,
    LoadingMetadata,
    DownloadingFiles,
    DownloadPaused,
    StoppingTorrent,
    Completed,
    Deleting,
    Failed,
}
