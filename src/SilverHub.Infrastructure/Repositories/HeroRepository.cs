using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SilverHub.Application.Heroes.Dtos;
using SilverHub.Application.Heroes.Ports;
using SilverHub.Application.Heroes.Queries;
using SilverHub.Domain.Heroes;
using SilverHub.Domain.Synergies;
using SilverHub.Infrastructure.Persistence;

namespace SilverHub.Infrastructure.Repositories;

public sealed class HeroRepository : IHeroRepository
{
    private readonly SilverHubDbContext _db;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public HeroRepository(SilverHubDbContext db) => _db = db;

    public async Task<IReadOnlyList<HeroListItemDto>> SearchAsync(HeroListQuery query, CancellationToken ct)
    {
        var q = _db.Heroes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Query))
        {
            var term = query.Query.Trim();
            q = q.Where(h =>
                EF.Functions.ILike(h.DisplayName, $"%{term}%") ||
                EF.Functions.ILike(h.CanonicalName, $"%{term}%") ||
                EF.Functions.ILike(h.Slug, $"%{term}%"));
        }

        if (!string.IsNullOrWhiteSpace(query.Rarity))
            q = q.Where(h => h.Rarity.ToString() == query.Rarity);

        if (!string.IsNullOrWhiteSpace(query.Faction))
            q = q.Where(h => h.Faction.ToString() == query.Faction);

        if (!string.IsNullOrWhiteSpace(query.Class))
            q = q.Where(h => h.Class.ToString() == query.Class);

        var sort = (query.Sort ?? string.Empty).Trim().ToLowerInvariant();
        q = sort switch
        {
            "newest" => q
                .OrderByDescending(h => h.ReleaseDate.HasValue)
                .ThenByDescending(h => h.ReleaseDate)
                .ThenByDescending(h => h.CreatedAt)
                .ThenBy(h => h.DisplayName),
            "updated" => q.OrderByDescending(h => h.UpdatedAt).ThenBy(h => h.DisplayName),
            _ => q.OrderBy(h => h.DisplayName),
        };

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);

        var items = await q.Select(h => new
        {
            h.Id,
            h.Slug,
            h.DisplayName,
            h.ReleaseDate,
            Rarity = h.Rarity.ToString(),
            Faction = h.Faction.ToString(),
            EquipType = h.EquipType.ToString(),
            Class = h.Class.ToString(),
            MoonType = h.MoonType.ToString(),
            DamageType = h.DamageType.ToString(),
            h.Limited,
            h.Boudoir,
            h.HasResonantia,
            PortraitKey = _db.HeroImages
                .Where(i => i.HeroId == h.Id && i.Kind == HeroImageKind.Portrait)
                .OrderBy(i => i.SortOrder)
                .Select(i => i.ImageKey)
                .FirstOrDefault(),
            PrimarySynergy = _db.SynergyMembers
                .Where(m => m.HeroId == h.Id)
                .OrderBy(m => m.Role)
                .ThenBy(m => m.SortOrder)
                .Select(m => new { m.Synergy.Slug, m.Synergy.Name })
                .FirstOrDefault()
        })
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(x => new HeroListItemDto(
            x.Id,
            x.Slug,
            x.DisplayName,
            x.ReleaseDate,
            x.Rarity,
            x.Faction,
            x.EquipType,
            x.Class,
            x.MoonType,
            x.DamageType,
            x.Limited,
            x.Boudoir,
            x.HasResonantia,
            x.PortraitKey,
            x.PrimarySynergy != null ? x.PrimarySynergy.Slug : null,
            x.PrimarySynergy != null ? x.PrimarySynergy.Name : null
        ))
        .ToListAsync(ct);

        return items;
    }

    public async Task<HeroDetailDto?> GetBySlugAsync(string slug, CancellationToken ct)
    {
        var hero = await _db.Heroes.AsNoTracking()
            .Where(h => h.Slug == slug)
            .Select(h => new
            {
                h.Id,
                h.Slug,
                h.DisplayName,
                h.CanonicalName,
                h.ReleaseDate,
                h.BreakdownMarkdown,
                h.HasResonantia,
                h.ResonantiaTraitsJson,
                Rarity = h.Rarity.ToString(),
                Faction = h.Faction.ToString(),
                EquipType = h.EquipType.ToString(),
                Class = h.Class.ToString(),
                MoonType = h.MoonType.ToString(),
                DamageType = h.DamageType.ToString(),
                h.Limited,
                h.Boudoir,
                h.FriendshipMax
            })
            .FirstOrDefaultAsync(ct);

        if (hero is null) return null;

        var images = await _db.HeroImages.AsNoTracking()
            .Where(i => i.HeroId == hero.Id)
            .OrderBy(i => i.SortOrder)
            .Select(i => new HeroImageDto(i.Id, i.Kind.ToString(), i.ImageKey, i.VariantName, i.SortOrder))
            .ToListAsync(ct);

        var skills = await _db.HeroSkills.AsNoTracking()
            .Where(s => s.HeroId == hero.Id)
            .OrderBy(s => s.SortOrder)
            .Select(s => new HeroSkillDto(
                s.Id,
                s.Type.ToString(),
                s.Name,
                s.IconKey,
                s.DescriptionMarkdown,
                s.Cost,
                s.ValuesJson,
                s.BuffsJson,
                s.DebuffsJson,
                s.SortOrder
            ))
            .ToListAsync(ct);

        var tags = await _db.HeroTags.AsNoTracking()
            .Where(t => t.HeroId == hero.Id)
            .Select(t => t.Tag)
            .OrderBy(t => t)
            .ToListAsync(ct);

        var artifacts = await _db.HeroPreferredArtifacts.AsNoTracking()
            .Where(a => a.HeroId == hero.Id)
            .OrderBy(a => a.SortOrder)
            .Select(a => new PreferredArtifactDto(a.ArtifactName, a.SortOrder))
            .ToListAsync(ct);

        var memberships = await _db.SynergyMembers.AsNoTracking()
            .Where(m => m.HeroId == hero.Id)
            .ToListAsync(ct);

        var synergies = new List<SynergyDto>();

        foreach (var membership in memberships)
        {
            var synergy = await _db.Synergies.AsNoTracking()
                .Where(s => s.Id == membership.SynergyId)
                .Select(s => new
                {
                    s.Id,
                    s.Slug,
                    s.Name,
                    s.IconKey,
                    s.Icon2Key,
                    s.Description2Markdown,
                    s.Icon3Key,
                    s.Description3Markdown
                })
                .FirstAsync(ct);

            var members = await _db.SynergyMembers.AsNoTracking()
                .Where(m => m.SynergyId == synergy.Id)
                .OrderBy(m => m.SortOrder)
                .ToListAsync(ct);

            var memberHeroIds = members.Select(m => m.HeroId).Distinct().ToList();

            var heroRefs = await _db.Heroes.AsNoTracking()
                .Where(h => memberHeroIds.Contains(h.Id))
                .Select(h => new { h.Id, h.Slug, h.DisplayName })
                .ToListAsync(ct);

            var avatarKeys = await _db.HeroImages.AsNoTracking()
                .Where(i => memberHeroIds.Contains(i.HeroId) && i.Kind == HeroImageKind.Avatar)
                .GroupBy(i => i.HeroId)
                .Select(g => new { HeroId = g.Key, AvatarKey = g.OrderBy(x => x.SortOrder).Select(x => x.ImageKey).FirstOrDefault() })
                .ToListAsync(ct);

            SynergyHeroRefDto MapRef(Guid heroId)
            {
                var h = heroRefs.First(x => x.Id == heroId);
                var avatar = avatarKeys.FirstOrDefault(x => x.HeroId == heroId)?.AvatarKey;
                return new SynergyHeroRefDto(h.Id, h.Slug, h.DisplayName, avatar);
            }

            var oath = members.FirstOrDefault(m => m.Role == SynergyRole.Oathsworn);
            var initiates = members
                .Where(m => m.Role == SynergyRole.Initiate)
                .Select(m => MapRef(m.HeroId))
                .ToList();

            synergies.Add(new SynergyDto(
                synergy.Id,
                synergy.Slug,
                synergy.Name,
                synergy.IconKey,
                synergy.Icon2Key,
                synergy.Description2Markdown,
                synergy.Icon3Key,
                synergy.Description3Markdown,
                membership.Role.ToString(),
                oath is null ? null : MapRef(oath.HeroId),
                initiates
            ));
        }

        var resonantiaTraits = BuildResonantiaTraits(hero.HasResonantia, hero.ResonantiaTraitsJson);

        return new HeroDetailDto(
            hero.Id,
            hero.Slug,
            hero.DisplayName,
            hero.CanonicalName,
            hero.ReleaseDate,
            hero.Rarity,
            hero.Faction,
            hero.EquipType,
            hero.Class,
            hero.MoonType,
            hero.DamageType,
            hero.Limited,
            hero.Boudoir,
            hero.FriendshipMax,
            hero.BreakdownMarkdown,
            hero.HasResonantia,
            resonantiaTraits,
            images,
            skills,
            tags,
            artifacts,
            synergies
        );
    }

    public async Task<IReadOnlyList<HeroOptionDto>> GetOptionsAsync(CancellationToken ct)
        => await _db.Heroes.AsNoTracking()
            .OrderBy(h => h.DisplayName)
            .Select(h => new HeroOptionDto(
                h.DisplayName,
                h.CanonicalName,
                h.Slug,
                _db.HeroImages
                    .Where(i => i.HeroId == h.Id && i.Kind == HeroImageKind.Avatar)
                    .OrderBy(i => i.SortOrder)
                    .Select(i => i.ImageKey)
                    .FirstOrDefault(),
                _db.HeroSkills
                    .Where(s => s.HeroId == h.Id && s.Type == SkillType.Ultimate)
                    .OrderBy(s => s.SortOrder)
                    .Select(s => s.Cost)
                    .FirstOrDefault(),
                h.MoonType.ToString()
            ))
            .ToListAsync(ct);

    private static IReadOnlyList<ResonantiaTraitDto> BuildResonantiaTraits(bool hasResonantia, string? heroSpecificJson)
    {
        if (!hasResonantia) return Array.Empty<ResonantiaTraitDto>();

        var result = new List<ResonantiaTraitDto>();

        if (!string.IsNullOrWhiteSpace(heroSpecificJson))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<List<ResonantiaTraitDto>>(heroSpecificJson, JsonOptions) ?? new();
                // Hero-specific traits never override the shared slots (2 and 5).
                result.AddRange(parsed.Where(t => t.Slot is not 2 and not 5));
            }
            catch
            {
                // Non-fatal: still render hero detail even if seed content is malformed.
            }
        }

        foreach (var shared in ResonantiaSharedTraits.All)
        {
            result.Add(new ResonantiaTraitDto(shared.Slot, shared.Name, shared.Description, shared.SpiritSiphon));
        }

        return result
            .GroupBy(t => t.Slot)
            .Select(g => g.First())
            .OrderBy(t => t.Slot)
            .ToList();
    }
}
