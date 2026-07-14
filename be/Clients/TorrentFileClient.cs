using be.Config;
using Microsoft.Extensions.Options;

namespace be.Clients;

public class TorrentFileClient(
    IOptions<LibraryConfig> config,
    IHttpClientFactory httpClientFactory)
{
    public string TorrentFile(string infoHash)
        => Path.Combine(torrentFilesDirectory, $"{infoHash}.torrent");

    public bool TorrentFileExists(string infoHash)
        => File.Exists(TorrentFile(infoHash));

    public async Task DownloadTorrentFileAsync(string infoHash, CancellationToken cancellationToken)
    {
        EnsureDirectoryExists();
        var torrentFilePath = TorrentFile(infoHash);
        if (File.Exists(torrentFilePath)) return;
        var tmpFilePath = $"{torrentFilePath}.tmp";
        var url = $"https://itorrents.net/torrent/{infoHash}.torrent";
        using var client = httpClientFactory.CreateClient();
        try
        {
            using (var readStream = await client.GetStreamAsync(url, cancellationToken))
            using (var fileStream = File.OpenWrite(tmpFilePath))
            {
                await readStream.CopyToAsync(fileStream, cancellationToken);
            }
            File.Move(tmpFilePath, torrentFilePath);
        }
        catch
        {
            if (File.Exists(tmpFilePath))
            {
                File.Delete(tmpFilePath);
            }
            throw;
        }
    }

    public void DeleteTorrentFile(string infoHash)
    {
        var torrentFilePath = TorrentFile(infoHash);
        if (File.Exists(torrentFilePath))
        {
            File.Delete(torrentFilePath);
        }
    }

    private readonly string torrentFilesDirectory = Path.Combine(config.Value.RootDirectory, ".torrents", "metadata");

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(torrentFilesDirectory))
        {
            Directory.CreateDirectory(torrentFilesDirectory);
        }
    }
}
