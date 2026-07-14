using System.Net;
using be.Interfaces;
using be.Models;

namespace be.GetMediaFile;

public class GetMediaFileService(IMediaFileRetriever mediaFileRetriever)
{
    public Result<MediaFile> GetMovieMediaFile(string titleId)
    {
        var mediaFile = mediaFileRetriever.GetMovieMediaFile(titleId);
        return mediaFile is not null
            ? Result.Success(mediaFile)
            : Result<MediaFile>.Failure("File not found", HttpStatusCode.NotFound);
    }

    public Result<MediaFile> GetEpisodeMediaFile(string titleId, int seasonNumber, int episodeNumber)
    {
        var mediaFile = mediaFileRetriever.GetEpisodeMediaFile(titleId, seasonNumber, episodeNumber);
        return mediaFile is not null
            ? Result.Success(mediaFile)
            : Result<MediaFile>.Failure("File not found", HttpStatusCode.NotFound);
    }
}
