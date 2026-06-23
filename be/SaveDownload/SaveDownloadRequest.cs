using be.Shared;

namespace be.SaveDownload;

public record SaveDownloadRequest(
    string InfoHash,
    MovieOrSeries Title);
