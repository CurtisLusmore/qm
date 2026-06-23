namespace be.Shared;

public record DownloadTracker(
    string InfoHash,
    MovieOrSeries Title,
    DownloadStatus Status = DownloadStatus.Received,
    string? Error = null,
    long DownloadedBytes = 0,
    long TargetBytes = 0,
    long TotalBytes = 0,
    double PartialProgressPercent = 0,
    double TargetProgressPercent = 0,
    double TotalProgressPercent = 0,
    double BytesPerSecond = 0,
    int Seeds = 0);
