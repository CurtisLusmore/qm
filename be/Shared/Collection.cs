namespace be.Shared;

public record Collection(
    IEnumerable<Movie> Movies,
    IEnumerable<Series> Series);
