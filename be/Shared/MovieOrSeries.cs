namespace be.Shared;

public record MovieOrSeries(
    string Id,
    TitleType Type,
    string Name,
    int? Year,
    string ImageUrl,
    int? EndYear,
    DateTime? ReleaseDate,
    string Plot,
    string[] Genres,
    string? TrailerUrl,
    string Classification,
    Ratings Ratings,
    PersonSummary[] Cast,
    PersonSummary[] Directors,
    PersonSummary[] Writers,
    Episode[]? Episodes);
