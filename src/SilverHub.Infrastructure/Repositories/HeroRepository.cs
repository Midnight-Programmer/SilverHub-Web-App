using Microsoft.EntityFrameworkCore;
using SilverHub.Application.Heroes.Dtos;
using SilverHub.Application.Heroes.Ports;
using SilverHub.Application.Heroes.Queries;
using SilverHub.Domain.Heroes;
using SilverHub.Infrastructure.Persistence;

namespace SilverHub.Infrastructure.Repositories;

public sealed class HeroRepository : IHeroRepository
{
    private readonly SilverHubDbContext _db;

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
}
