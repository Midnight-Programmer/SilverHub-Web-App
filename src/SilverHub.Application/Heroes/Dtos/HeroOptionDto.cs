namespace SilverHub.Application.Heroes.Dtos;

public sealed record HeroOptionDto(
    string DisplayName,
    string CanonicalName,
    string Slug,
    string? IconKey,
    int? UltimateCost,
    string MoonType
);
