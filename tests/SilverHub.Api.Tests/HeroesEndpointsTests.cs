using System.Net;
using System.Net.Http.Json;

namespace SilverHub.Api.Tests;

public sealed class HeroesEndpointsTests : IClassFixture<HeroesApiFactory>
{
    private readonly HttpClient _client;

    public HeroesEndpointsTests(HeroesApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task HealthReady_ReturnsOk()
    {
        var response = await _client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetHeroes_ReturnsSeededHeroes()
    {
        var heroes = await _client.GetFromJsonAsync<List<HeroListItemResponse>>("/api/v1/heroes");

        Assert.NotNull(heroes);
        Assert.NotEmpty(heroes);
        Assert.Contains(heroes!, h => h.Slug == "ethereal-joan");
    }

    [Fact]
    public async Task GetHeroBySlug_ReturnsNotFound_WhenSlugDoesNotExist()
    {
        var response = await _client.GetAsync("/api/v1/heroes/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private sealed record HeroListItemResponse(string Slug, string DisplayName);
}
