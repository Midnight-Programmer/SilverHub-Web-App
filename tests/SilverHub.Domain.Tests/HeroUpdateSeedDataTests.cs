using SilverHub.Domain.Heroes;

namespace SilverHub.Domain.Tests;

public class HeroUpdateSeedDataTests
{
    private static Hero NewJoan() => new(
        id: Guid.NewGuid(),
        slug: "joan",
        displayName: "Joan",
        canonicalName: "Joan",
        rarity: Rarity.SSR,
        faction: Faction.Church,
        equipType: EquipType.Heavy,
        heroClass: HeroClass.Defender,
        moonType: MoonType.Full,
        damageType: DamageType.Physical,
        boudoir: false,
        limited: false,
        friendshipMax: 100,
        releaseDate: new DateOnly(2024, 1, 15),
        breakdownMarkdown: null,
        hasResonantia: false,
        resonantiaTraitsJson: null);

    [Fact]
    public void UpdateSeedData_ReturnsFalse_WhenNothingChanges()
    {
        var hero = NewJoan();

        var changed = hero.UpdateSeedData(
            slug: "joan",
            displayName: "Joan",
            canonicalName: "Joan",
            rarity: Rarity.SSR,
            faction: Faction.Church,
            equipType: EquipType.Heavy,
            heroClass: HeroClass.Defender,
            moonType: MoonType.Full,
            damageType: DamageType.Physical,
            boudoir: false,
            limited: false,
            friendshipMax: 100,
            releaseDate: new DateOnly(2024, 1, 15),
            breakdownMarkdown: null,
            hasResonantia: false,
            resonantiaTraitsJson: null);

        Assert.False(changed);
    }

    [Fact]
    public void UpdateSeedData_ReturnsTrueAndUpdatesField_WhenDisplayNameChanges()
    {
        var hero = NewJoan();

        var changed = hero.UpdateSeedData(
            slug: "joan",
            displayName: "Ethereal Joan",
            canonicalName: "Joan",
            rarity: Rarity.SSR,
            faction: Faction.Church,
            equipType: EquipType.Heavy,
            heroClass: HeroClass.Defender,
            moonType: MoonType.Full,
            damageType: DamageType.Physical,
            boudoir: false,
            limited: false,
            friendshipMax: 100,
            releaseDate: new DateOnly(2024, 1, 15),
            breakdownMarkdown: null,
            hasResonantia: false,
            resonantiaTraitsJson: null);

        Assert.True(changed);
        Assert.Equal("Ethereal Joan", hero.DisplayName);
    }
}
