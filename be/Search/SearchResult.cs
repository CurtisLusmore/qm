namespace be.Search;

public record SearchResult(
  string InfoHash,
  string Name,
  long SizeBytes,
  int Seeders);
