using SilverHub.Application.Heroes.Dtos;
using SilverHub.Application.Heroes.Queries;

namespace SilverHub.Application.Heroes.Ports;

public interface IHeroRepository
{
    Task<IReadOnlyList<HeroListItemDto>> SearchAsync(HeroListQuery query, CancellationToken ct);
}
