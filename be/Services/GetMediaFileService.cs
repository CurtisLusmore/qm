using be.Interfaces;
using be.Models;

namespace be.Services;

public partial class DownloadManagementService : IMediaFileRetriever
{
    public MediaFile? GetMovieMediaFile(string titleId)
    {
        return fileService.GetMovieMediaFile(titleId);
    }

    public MediaFile? GetEpisodeMediaFile(string titleId, int seasonNumber, int episodeNumber)
    {
        return fileService.GetEpisodeMediaFile(titleId, seasonNumber, episodeNumber);
    }
}
