using Microsoft.AspNetCore.Mvc;
using SilverHub.Application.Heroes.Queries;

namespace SilverHub.Api.Controllers;

[ApiController]
[Route("api/v1/heroes")]
public sealed class HeroesController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] string? query,
        [FromQuery] string? rarity,
        [FromQuery] string? faction,
        [FromQuery] string? @class,
        [FromQuery] string? sort,
        [FromServices] GetHeroesListHandler handler,
        CancellationToken ct,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await handler.HandleAsync(
            new HeroListQuery(query, rarity, faction, @class, sort, page, pageSize), ct);

        return Ok(result);
    }

    [HttpGet("options")]
    public async Task<IActionResult> GetOptions(
        [FromServices] GetHeroOptionsHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(ct);
        return Ok(result);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(
        [FromRoute] string slug,
        [FromServices] GetHeroBySlugHandler handler,
        CancellationToken ct = default)
    {
        var hero = await handler.HandleAsync(slug, ct);
        return hero is null ? NotFound() : Ok(hero);
    }
}
