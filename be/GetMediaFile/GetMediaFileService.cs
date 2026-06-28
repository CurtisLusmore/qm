using be.Interfaces;
using be.Shared;

namespace be.GetMediaFile;

public class GetMediaFileService(IMediaFileRetriever mediaFileRetriever)
{
    public Result<MediaFile> GetMovieMediaFile(string titleId)
    {
        var mediaFile = mediaFileRetriever.GetMovieMediaFile(titleId);
        return mediaFile != null
            ? Result<MediaFile>.Success(mediaFile)
            : Result<MediaFile>.Failure("File not found", 404);
    }

    public Result<MediaFile> GetEpisodeMediaFile(string titleId, int seasonNumber, int episodeNumber)
    {
        var mediaFile = mediaFileRetriever.GetEpisodeMediaFile(titleId, seasonNumber, episodeNumber);
        return mediaFile != null
            ? Result<MediaFile>.Success(mediaFile)
            : Result<MediaFile>.Failure("File not found", 404);
    }
}
