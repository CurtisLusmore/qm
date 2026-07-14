namespace be.Models;

public record DownloadFile(
    string Path,
    FilePriority Priority,
    long DownloadedBytes,
    long TotalBytes,
    double ProgressPercent);
