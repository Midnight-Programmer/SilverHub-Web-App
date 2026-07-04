using SilverHub.Application.Heroes.Dtos;
using SilverHub.Application.Heroes.Ports;

namespace SilverHub.Application.Heroes.Queries;

public sealed class GetHeroOptionsHandler
{
    private readonly IHeroRepository _repo;

    public GetHeroOptionsHandler(IHeroRepository repo) => _repo = repo;

    public Task<IReadOnlyList<HeroOptionDto>> HandleAsync(CancellationToken ct)
        => _repo.GetOptionsAsync(ct);
}
