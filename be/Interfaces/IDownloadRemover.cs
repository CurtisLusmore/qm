namespace be.Interfaces;

public interface IDownloadRemover
{
    Task RequestRemoveDownloadAsync(string infoHash);
}
