namespace SilverHub.Domain.Heroes;

public static class ResonantiaSharedTraits
{
    public static readonly ResonantiaTraitDefinition BloodMemory = new(
        Slot: 2,
        Name: "Blood Memory",
        Description: "When the battle begins, increases own HP by 15/20/25%, All DMG Reduction by 15/20/25%, and CRIT Resist Rate by 15/20/25%.",
        SpiritSiphon: 3
    );

    public static readonly ResonantiaTraitDefinition Severance = new(
        Slot: 5,
        Name: "Severance",
        Description: "When the battle begins, increases own HP by 35/40/45%, ATK by 4/5/6%, P. DEF by 4/5/6%, and M. DEF by 4/5/6%.",
        SpiritSiphon: 9
    );

    public static IReadOnlyList<ResonantiaTraitDefinition> All => new[]
    {
        BloodMemory,
        Severance,
    };
}
