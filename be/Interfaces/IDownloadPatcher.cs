using be.Models;

namespace be.Interfaces;

public interface IDownloadPatcher
{
    Task RequestPatchDownloadAsync(string infoHash, DownloadPatch patch);
}
