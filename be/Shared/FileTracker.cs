namespace be.Shared;

public record FileTracker(
    string Path,
    FilePriority Priority,
    long DownloadedBytes = 0,
    long TotalBytes = 0,
    double ProgressPercent = 0);
