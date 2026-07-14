using be.Models;

namespace be.Interfaces;

public interface IDownloadSaver
{
    Task SaveDownloadAsync(string infoHash, MovieOrSeries title, CancellationToken cancellationToken);
}
