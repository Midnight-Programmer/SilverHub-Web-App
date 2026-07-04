namespace SilverHub.Application.Heroes.Dtos;

public sealed record HeroDetailDto(
    Guid Id,
    string Slug,
    string DisplayName,
    string CanonicalName,
    DateOnly? ReleaseDate,
    string Rarity,
    string Faction,
    string EquipType,
    string Class,
    string MoonType,
    string DamageType,
    bool Limited,
    bool Boudoir,
    int FriendshipMax,
    string? BreakdownMarkdown,
    bool HasResonantia,
    IReadOnlyList<ResonantiaTraitDto> ResonantiaTraits,
    IReadOnlyList<HeroImageDto> Images,
    IReadOnlyList<HeroSkillDto> Skills,
    IReadOnlyList<string> Tags,
    IReadOnlyList<PreferredArtifactDto> PreferredArtifacts,
    IReadOnlyList<SynergyDto> Synergies
);

public sealed record ResonantiaTraitDto(
    int Slot,
    string Name,
    string Description,
    int SpiritSiphon
);

public sealed record HeroImageDto(
    Guid Id,
    string Kind,
    string ImageKey,
    string? VariantName,
    int SortOrder
);

public sealed record HeroSkillDto(
    Guid Id,
    string SkillType,
    string Name,
    string? IconKey,
    string DescriptionMarkdown,
    int? Cost,
    string? ValuesJson,
    string? BuffsJson,
    string? DebuffsJson,
    int SortOrder
);

public sealed record PreferredArtifactDto(
    string Name,
    int SortOrder
);

public sealed record SynergyDto(
    Guid Id,
    string Slug,
    string Name,
    string? IconKey,
    string? Icon2Key,
    string? Description2Markdown,
    string? Icon3Key,
    string? Description3Markdown,
    string Role,
    SynergyHeroRefDto? Oathsworn,
    IReadOnlyList<SynergyHeroRefDto> Initiates
);

public sealed record SynergyHeroRefDto(
    Guid Id,
    string Slug,
    string DisplayName,
    string? AvatarImageKey
);
