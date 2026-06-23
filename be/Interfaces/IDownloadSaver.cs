using be.Shared;

namespace be.Interfaces;

public interface IDownloadSaver
{
    Task SaveDownloadAsync(string infoHash, MovieOrSeries title);
}
