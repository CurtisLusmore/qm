using be.Config;
using Microsoft.Extensions.Options;

namespace be.Clients;

public class FastResumeFileClient(
    IOptions<LibraryConfig> config)
{
    public string FastResumeFile(string infoHash)
        => Path.Combine(config.Value.RootDirectory, ".torrents", "fastresume", $"{infoHash}.fresume");

    public void DeleteFastResumeFile(string infoHash)
    {
        var fastResumeFilePath = FastResumeFile(infoHash);
        if (File.Exists(fastResumeFilePath))
        {
            File.Delete(fastResumeFilePath);
        }
    }

}