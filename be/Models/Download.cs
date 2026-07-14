namespace be.Models;

public record Download(
    string Name,
    string InfoHash,
    MovieOrSeries Title,
    DownloadStatus Status,
    string? Error,
    IEnumerable<DownloadFile> Files,
    long DownloadedBytes,
    long TargetBytes,
    long TotalBytes,
    double PartialProgressPercent,
    double TargetProgressPercent,
    double TotalProgressPercent,
    double BytesPerSecond,
    int Seeds);
