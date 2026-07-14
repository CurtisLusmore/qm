using be.Models;

namespace be.SaveDownload;

public record SaveDownloadRequest(
    string InfoHash,
    MovieOrSeries Title);
