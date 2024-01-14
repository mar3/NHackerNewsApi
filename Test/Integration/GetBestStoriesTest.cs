using FluentAssertions;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NHackerNewsClient;
using WireMock.Server;
using ZiggyCreatures.Caching.Fusion;

namespace Test.Integration;

[UsesVerify]
public class GetBestStoriesTest : IClassFixture<AppFactory>, IAsyncLifetime
{
    private readonly WireMockServer _wireMockServer;
    private readonly HttpClient _client;
    private readonly BestStoriesService _service;

    public GetBestStoriesTest(AppFactory factory)
    {
        _wireMockServer = factory.WireMockServer;
        _service = new BestStoriesService(new CachedHackerNewsClient(
            factory.Services.GetRequiredService<IFusionCache>(),
            factory.Services.GetRequiredService<HttpHackerNewsClient>()));
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetBestStoriesShouldReturnNBestStories()
    {
        // given
        const int n = 10;
        
        // when
        var response = await $"beststories?n={n}"
            .WithClient(new FlurlClient(_client))
            .AllowAnyHttpStatus()
            .GetAsync();
        
        // then
        await Verify(new
        {
            response.StatusCode,
            Body = await response.GetJsonListAsync(),
        });
    }
    
    [Fact]
    public async Task GetBestStoriesShouldReturnBadRequestWhenNIsNegative()
    {
        // given
        const int n = -1;
        
        // when
        var response = await $"beststories?n={n}"
            .WithClient(new FlurlClient(_client))
            .AllowAnyHttpStatus()
            .GetAsync();
        
        // then
        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }
    
    [Fact]
    public async Task GetBestStoriesShouldUseCache()
    {
        // when
        await foreach (var _ in _service.GetBestStoriesAsync(10)) {}
        await foreach (var _ in _service.GetBestStoriesAsync(12)) {}
        
        // then
        _wireMockServer.LogEntries.Should().HaveCount(13);
    }
    
    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        _wireMockServer.ResetLogEntries();
        return Task.CompletedTask;
    }
}