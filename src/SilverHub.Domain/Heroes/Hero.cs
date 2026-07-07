using SilverHub.Domain.Synergies;

namespace SilverHub.Domain.Heroes;

public sealed class Hero
{
    private readonly List<HeroImage> _images = new();
    private readonly List<HeroSkill> _skills = new();
    private readonly List<HeroTag> _tags = new();
    private readonly List<HeroPreferredArtifact> _preferredArtifacts = new();
    private readonly List<SynergyMember> _synergyMemberships = new();

    private Hero() { }

    public Hero(
        Guid id,
        string slug,
        string displayName,
        string canonicalName,
        Rarity rarity,
        Faction faction,
        EquipType equipType,
        HeroClass heroClass,
        MoonType moonType,
        DamageType damageType,
        bool boudoir,
        bool limited,
        int friendshipMax,
        DateOnly? releaseDate,
        string? breakdownMarkdown,
        bool hasResonantia,
        string? resonantiaTraitsJson
    )
    {
        Id = id;
        Slug = slug;
        DisplayName = displayName;
        CanonicalName = canonicalName;
        Rarity = rarity;
        Faction = faction;
        EquipType = equipType;
        Class = heroClass;
        MoonType = moonType;
        DamageType = damageType;
        Boudoir = boudoir;
        Limited = limited;
        FriendshipMax = friendshipMax;
        ReleaseDate = releaseDate;
        BreakdownMarkdown = breakdownMarkdown;

        HasResonantia = hasResonantia;
        ResonantiaTraitsJson = resonantiaTraitsJson;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Slug { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public string CanonicalName { get; private set; } = default!;

    public Rarity Rarity { get; private set; }
    public Faction Faction { get; private set; }
    public EquipType EquipType { get; private set; }
    public HeroClass Class { get; private set; }
    public MoonType MoonType { get; private set; }
    public DamageType DamageType { get; private set; }

    public bool Boudoir { get; private set; }
    public bool Limited { get; private set; }
    public int FriendshipMax { get; private set; }

    public DateOnly? ReleaseDate { get; private set; }
    public string? BreakdownMarkdown { get; private set; }

    public bool HasResonantia { get; private set; }
    public string? ResonantiaTraitsJson { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<HeroImage> Images => _images;
    public IReadOnlyCollection<HeroSkill> Skills => _skills;
    public IReadOnlyCollection<HeroTag> Tags => _tags;
    public IReadOnlyCollection<HeroPreferredArtifact> PreferredArtifacts => _preferredArtifacts;

    public IReadOnlyCollection<SynergyMember> SynergyMemberships => _synergyMemberships;

    public void Touch() => UpdatedAt = DateTimeOffset.UtcNow;

    public bool UpdateSeedData(
        string slug,
        string displayName,
        string canonicalName,
        Rarity rarity,
        Faction faction,
        EquipType equipType,
        HeroClass heroClass,
        MoonType moonType,
        DamageType damageType,
        bool boudoir,
        bool limited,
        int friendshipMax,
        DateOnly? releaseDate,
        string? breakdownMarkdown,
        bool hasResonantia,
        string? resonantiaTraitsJson
    )
    {
        var changed = false;

        if (!string.Equals(Slug, slug, StringComparison.Ordinal)) { Slug = slug; changed = true; }
        if (!string.Equals(DisplayName, displayName, StringComparison.Ordinal)) { DisplayName = displayName; changed = true; }
        if (!string.Equals(CanonicalName, canonicalName, StringComparison.Ordinal)) { CanonicalName = canonicalName; changed = true; }

        if (Rarity != rarity) { Rarity = rarity; changed = true; }
        if (Faction != faction) { Faction = faction; changed = true; }
        if (EquipType != equipType) { EquipType = equipType; changed = true; }
        if (Class != heroClass) { Class = heroClass; changed = true; }
        if (MoonType != moonType) { MoonType = moonType; changed = true; }
        if (DamageType != damageType) { DamageType = damageType; changed = true; }
        if (Boudoir != boudoir) { Boudoir = boudoir; changed = true; }
        if (Limited != limited) { Limited = limited; changed = true; }
        if (FriendshipMax != friendshipMax) { FriendshipMax = friendshipMax; changed = true; }

        if (ReleaseDate != releaseDate) { ReleaseDate = releaseDate; changed = true; }
        if (!string.Equals(BreakdownMarkdown ?? string.Empty, breakdownMarkdown ?? string.Empty, StringComparison.Ordinal))
        {
            BreakdownMarkdown = breakdownMarkdown;
            changed = true;
        }

        if (HasResonantia != hasResonantia) { HasResonantia = hasResonantia; changed = true; }
        if (!string.Equals(ResonantiaTraitsJson ?? string.Empty, resonantiaTraitsJson ?? string.Empty, StringComparison.Ordinal))
        {
            ResonantiaTraitsJson = resonantiaTraitsJson;
            changed = true;
        }

        if (changed) Touch();
        return changed;
    }
}
