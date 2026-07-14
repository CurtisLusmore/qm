namespace be.Models;

public record Event(
    string Type);

public record DownloadAddedEvent(
    Download Download)
    : Event("DownloadAdded");

public record DownloadProgressEvent(
    Download Download)
    : Event("DownloadProgress");

public record DownloadCompletedEvent(
    Download Download)
    : Event("DownloadCompleted");

public record DownloadFailedEvent(
    Download Download)
    : Event("DownloadFailed");

public record DownloadRemoved(
    string InfoHash)
    : Event("DownloadRemoved");

public record MovieAddedEvent(
    Movie Movie)
    : Event("MovieAdded");

public record SeriesAddedEvent(
    Series Series)
    : Event("SeriesAdded");
