namespace SilverHub.Application.Heroes.Queries;

public sealed record HeroListQuery(
    string? Query,
    string? Rarity,
    string? Faction,
    string? Class,
    string? Sort,
    int Page = 1,
    int PageSize = 50
);
