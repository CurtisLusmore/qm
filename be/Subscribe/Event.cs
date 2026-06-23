using be.Shared;

namespace be.Subscribe;

public record Event(
    IEnumerable<DownloadTracker> Downloads);
