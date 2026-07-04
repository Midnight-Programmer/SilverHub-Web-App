using SilverHub.Application.Heroes.Dtos;
using SilverHub.Application.Heroes.Ports;

namespace SilverHub.Application.Heroes.Queries;

public sealed class GetHeroesListHandler
{
    private readonly IHeroRepository _repo;

    public GetHeroesListHandler(IHeroRepository repo) => _repo = repo;

    public Task<IReadOnlyList<HeroListItemDto>> HandleAsync(HeroListQuery query, CancellationToken ct)
        => _repo.SearchAsync(query, ct);
}
