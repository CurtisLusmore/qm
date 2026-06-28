namespace be.Shared;

public record MediaFile(
    string TitleId,
    string FilePath,
    string MediaType,
    int? SeasonNumber = null,
    int? EpisodeNumber = null);
