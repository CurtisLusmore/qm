using be.Models;

namespace be.Interfaces;

public interface IMediaFileRetriever
{
    MediaFile? GetMovieMediaFile(string titleId);
    MediaFile? GetEpisodeMediaFile(string titleId, int seasonNumber, int episodeNumber);
}
