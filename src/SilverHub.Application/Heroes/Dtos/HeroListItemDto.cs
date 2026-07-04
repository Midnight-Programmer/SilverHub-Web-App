namespace SilverHub.Application.Heroes.Dtos;

public sealed record HeroListItemDto(
    Guid Id,
    string Slug,
    string DisplayName,
    DateOnly? ReleaseDate,
    string Rarity,
    string Faction,
    string EquipType,
    string Class,
    string MoonType,
    string DamageType,
    bool Limited,
    bool Boudoir,
    bool HasResonantia,
    string? PortraitImageKey,
    string? SynergySlug,
    string? SynergyName
);
