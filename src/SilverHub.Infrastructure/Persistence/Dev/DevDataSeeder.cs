using Microsoft.EntityFrameworkCore;
using SilverHub.Domain.Heroes;
using SilverHub.Domain.Synergies;

namespace SilverHub.Infrastructure.Persistence.Dev;

public static class DevDataSeeder
{
    public static async Task SeedAsync(SilverHubDbContext db, CancellationToken ct = default)
    {
        if (await db.Heroes.AnyAsync(ct)) return;

        var joanId = Guid.NewGuid();
        var valenId = Guid.NewGuid();
        var seraphineId = Guid.NewGuid();
        var kaelId = Guid.NewGuid();
        var lyraId = Guid.NewGuid();
        var thoralfId = Guid.NewGuid();

        db.Heroes.AddRange(
            new Hero(joanId, "ethereal-joan", "Ethereal Joan", "Joan", Rarity.SSR, Faction.Church, EquipType.Heavy, HeroClass.Defender, MoonType.Full, DamageType.Physical, boudoir: true, limited: true, friendshipMax: 100, releaseDate: new DateOnly(2024, 1, 15), breakdownMarkdown: "## Ethereal Joan\nA premier defender who shields the team and refuses to fall.", hasResonantia: true, resonantiaTraitsJson: null),
            new Hero(valenId, "valen", "Valen", "Valen", Rarity.R, Faction.Kingdom, EquipType.Light, HeroClass.Marksman, MoonType.New, DamageType.Piercing, boudoir: false, limited: false, friendshipMax: 100, releaseDate: null, breakdownMarkdown: null, hasResonantia: false, resonantiaTraitsJson: null),
            new Hero(seraphineId, "seraphine", "Seraphine", "Seraphine", Rarity.SSR, Faction.Church, EquipType.Medium, HeroClass.Enchanter, MoonType.Crescent, DamageType.Magic, boudoir: false, limited: false, friendshipMax: 100, releaseDate: new DateOnly(2023, 11, 2), breakdownMarkdown: null, hasResonantia: false, resonantiaTraitsJson: null),
            new Hero(kaelId, "kael", "Kael", "Kael", Rarity.SR, Faction.Bloodborn, EquipType.Heavy, HeroClass.Warrior, MoonType.Full, DamageType.Physical, boudoir: false, limited: false, friendshipMax: 100, releaseDate: new DateOnly(2023, 6, 20), breakdownMarkdown: null, hasResonantia: false, resonantiaTraitsJson: null),
            new Hero(lyraId, "lyra", "Lyra", "Lyra", Rarity.SR, Faction.Ancestry, EquipType.Light, HeroClass.Assassin, MoonType.New, DamageType.Physical, boudoir: false, limited: false, friendshipMax: 100, releaseDate: new DateOnly(2023, 8, 9), breakdownMarkdown: null, hasResonantia: false, resonantiaTraitsJson: null),
            new Hero(thoralfId, "thoralf", "Thoralf", "Thoralf", Rarity.SSR, Faction.Otherworldly, EquipType.Medium, HeroClass.Sorcerer, MoonType.Crescent, DamageType.Magic, boudoir: false, limited: true, friendshipMax: 100, releaseDate: new DateOnly(2024, 3, 30), breakdownMarkdown: null, hasResonantia: false, resonantiaTraitsJson: null)
        );

        // Portraits/avatars. Valen intentionally has no images -> null portrait in the list.
        db.HeroImages.AddRange(
            new HeroImage(Guid.NewGuid(), joanId, HeroImageKind.Portrait, "heroes/ethereal-joan/portrait.webp", null, 0),
            new HeroImage(Guid.NewGuid(), joanId, HeroImageKind.Avatar, "heroes/ethereal-joan/avatar.webp", null, 0),
            new HeroImage(Guid.NewGuid(), seraphineId, HeroImageKind.Portrait, "heroes/seraphine/portrait.webp", null, 0),
            new HeroImage(Guid.NewGuid(), seraphineId, HeroImageKind.Avatar, "heroes/seraphine/avatar.webp", null, 0),
            new HeroImage(Guid.NewGuid(), kaelId, HeroImageKind.Portrait, "heroes/kael/portrait.webp", null, 0),
            new HeroImage(Guid.NewGuid(), lyraId, HeroImageKind.Portrait, "heroes/lyra/portrait.webp", null, 0),
            new HeroImage(Guid.NewGuid(), thoralfId, HeroImageKind.Portrait, "heroes/thoralf/portrait.webp", null, 0)
        );

        // Ethereal Joan: fully populated child data (exercises the detail endpoint later).
        db.HeroSkills.AddRange(
            new HeroSkill(Guid.NewGuid(), joanId, SkillType.Basic, "Radiant Strike", "heroes/ethereal-joan/skill-basic.webp", "Deals **120%** ATK to the nearest enemy.", null, null, null, null, 0),
            new HeroSkill(Guid.NewGuid(), joanId, SkillType.Ultimate, "Hallowed Aegis", "heroes/ethereal-joan/skill-ult.webp", "Shields all allies for **30%** of Joan's max HP for 2 turns.", 1000, null, null, null, 1),
            new HeroSkill(Guid.NewGuid(), joanId, SkillType.Special, "Consecration", "heroes/ethereal-joan/skill-special.webp", "Taunts all enemies and gains **40%** damage reduction.", 400, null, null, null, 2),
            new HeroSkill(Guid.NewGuid(), joanId, SkillType.Talent, "Undying Faith", null, "Revives once per battle at **25%** HP.", null, null, null, null, 3)
        );
        db.HeroTags.AddRange(
            new HeroTag(joanId, "defender"),
            new HeroTag(joanId, "shield"),
            new HeroTag(joanId, "sustain")
        );
        db.HeroPreferredArtifacts.AddRange(
            new HeroPreferredArtifact(joanId, "Acappella", "acappella", 0),
            new HeroPreferredArtifact(joanId, "Hikami's Grace", "hikamis-grace", 1)
        );

        // Synergy: Seraphine (Oathsworn) + Ethereal Joan (Initiate).
        var synergyId = Guid.NewGuid();
        db.Synergies.Add(new Synergy(synergyId, "ethereal-bond", "Ethereal Bond", "synergies/ethereal-bond/icon.webp", null, null, null, null));
        db.SynergyMembers.AddRange(
            new SynergyMember(synergyId, seraphineId, SynergyRole.Oathsworn, 0),
            new SynergyMember(synergyId, joanId, SynergyRole.Initiate, 1)
        );

        await db.SaveChangesAsync(ct);
    }
}
