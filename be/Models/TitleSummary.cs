namespace be.Models;

public record TitleSummary(
    string Id,
    TitleType Type,
    string Name,
    int? Year);
