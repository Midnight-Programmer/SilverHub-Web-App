using SilverHub.Application.Heroes.Dtos;
using SilverHub.Application.Heroes.Ports;

namespace SilverHub.Application.Heroes.Queries;

public sealed class GetHeroBySlugHandler
{
    private readonly IHeroRepository _repo;

    public GetHeroBySlugHandler(IHeroRepository repo) => _repo = repo;

    public Task<HeroDetailDto?> HandleAsync(string slug, CancellationToken ct)
        => _repo.GetBySlugAsync(slug, ct);
}
