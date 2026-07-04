using SilverHub.Application.Heroes.Dtos;
using SilverHub.Application.Heroes.Queries;

namespace SilverHub.Application.Heroes.Ports;

public interface IHeroRepository
{
    Task<IReadOnlyList<HeroListItemDto>> SearchAsync(HeroListQuery query, CancellationToken ct);
    Task<HeroDetailDto?> GetBySlugAsync(string slug, CancellationToken ct);
    Task<IReadOnlyList<HeroOptionDto>> GetOptionsAsync(CancellationToken ct);
}
