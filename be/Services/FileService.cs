using be.Clients;
using be.Models;

namespace be.Services;

public class FileService(
    TitleFileClient titleFileClient,
    TorrentFileClient torrentFileClient,
    DownloadFileClient downloadFileClient,
    FileMappingClient fileMappingClient,
    FastResumeFileClient fastResumeFileClient)
{
    public IEnumerable<string> ListInfoHashes()
        => titleFileClient.ListInfoHashes();

    public async Task SaveTitleAsync(string infoHash, MovieOrSeries title, CancellationToken cancellationToken)
        => await titleFileClient.SaveTitleFileAsync(infoHash, title, cancellationToken);

    public async Task SaveMovieAsync(Movie title, CancellationToken cancellationToken)
        => await titleFileClient.SaveMovieAsync(title, cancellationToken);

    public async Task SaveSeriesAsync(Series title, CancellationToken cancellationToken)
        => await titleFileClient.SaveSeriesAsync(title, cancellationToken);

    public IAsyncEnumerable<Movie> ListMoviesAsync(DateTime addedSince, CancellationToken cancellationToken)
        => titleFileClient.ListMoviesAsync(addedSince, cancellationToken);

    public IAsyncEnumerable<Series> ListSeriesAsync(DateTime addedSince, CancellationToken cancellationToken)
        => titleFileClient.ListSeriesAsync(addedSince, cancellationToken);

    public async Task<MovieOrSeries> LoadTitleAsync(string infoHash, CancellationToken cancellationToken)
        => await titleFileClient.LoadTitleFileAsync(infoHash, cancellationToken);

    public async Task DownloadTorrentFileAsync(string infoHash, CancellationToken cancellationToken)
        => await torrentFileClient.DownloadTorrentFileAsync(infoHash, cancellationToken);

    public string TorrentFile(string infoHash)
        => torrentFileClient.TorrentFile(infoHash);

    public bool TorrentFileExists(string infoHash)
        => torrentFileClient.TorrentFileExists(infoHash);

    public void DeleteTorrentFile(string infoHash)
        => torrentFileClient.DeleteTorrentFile(infoHash);

    public string DownloadingDirectory(string infoHash, MovieOrSeries title)
        => downloadFileClient.DownloadingDirectory(infoHash, title);

    public void DeleteTorrentFiles(string infoHash)
    {
        titleFileClient.DeleteTitleFile(infoHash);
        torrentFileClient.DeleteTorrentFile(infoHash);
        fastResumeFileClient.DeleteFastResumeFile(infoHash);
    }

    public void DeleteDownloadingFiles(string infoHash, MovieOrSeries title)
        => downloadFileClient.DeleteDownloadingFiles(infoHash, title);

    public void MoveCompletedFiles(string infoHash, MovieOrSeries title, IEnumerable<FileMapping> fileMappings)
        => downloadFileClient.MoveCompletedFiles(infoHash, title, fileMappings);

    public string FastResumeFile(string infoHash)
        => fastResumeFileClient.FastResumeFile(infoHash);

    public IEnumerable<FileMapping> MapFiles(MovieOrSeries title, IEnumerable<MonoTorrent.ITorrentManagerFile> files)
        => fileMappingClient.MapFiles(title, files);

    public Task MarkMovieDownloadedAsync(TitleSummary title, CancellationToken cancellationToken)
        => titleFileClient.MarkMovieDownloadedAsync(title, cancellationToken);

    public Task MarkEpisodesDownloadedAsync(TitleSummary title, IEnumerable<EpisodeInfo> episodes, CancellationToken cancellationToken)
        => titleFileClient.MarkEpisodesDownloadedAsync(title, episodes, cancellationToken);

    public MediaFile? GetMovieMediaFile(string titleId)
        => downloadFileClient.GetMovieMediaFile(titleId);

    public MediaFile? GetEpisodeMediaFile(string titleId, int seasonNumber, int episodeNumber)
        => downloadFileClient.GetEpisodeMediaFile(titleId, seasonNumber, episodeNumber);

    public Task RemoveMovieAsync(string titleId)
        => titleFileClient.RemoveMovieAsync(titleId);

    public Task RemoveSeriesAsync(string titleId)
        => titleFileClient.RemoveSeriesAsync(titleId);
}
