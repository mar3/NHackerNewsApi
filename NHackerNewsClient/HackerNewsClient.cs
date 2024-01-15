using ZiggyCreatures.Caching.Fusion;

namespace NHackerNewsClient;

public interface IHackerNewsClient
{
    Task<IEnumerable<int>> GetBestStoryIdsAsync(CancellationToken cancellationToken);
    Task<Story> GetStoryAsync(int id, CancellationToken cancellationToken);
}

public sealed class CachedHackerNewsClient(IFusionCache cache, IHackerNewsClient client) : IHackerNewsClient
{
    public async Task<IEnumerable<int>> GetBestStoryIdsAsync(CancellationToken cancellationToken) =>
        (await cache.GetOrSetAsync<IEnumerable<int>>(
            "bestStoryIds",
            async ctx => await client.GetBestStoryIdsAsync(ctx),
            token: cancellationToken))!;

    public Task<Story> GetStoryAsync(int id, CancellationToken cancellationToken) =>
        cache.GetOrSetAsync<Story>(
            $"story-{id}",
            async ctx => await client.GetStoryAsync(id, ctx), 
            token: cancellationToken).AsTask()!;
}

public sealed class HttpHackerNewsClient(HttpClient httpClient) : IHackerNewsClient
{
    public async Task<IEnumerable<int>> GetBestStoryIdsAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync("/v0/beststories.json", cancellationToken);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<IEnumerable<int>>(cancellationToken)
            ?? throw new InvalidOperationException("Cannot deserialize best stories");
    }

    public async Task<Story> GetStoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"/v0/item/{id}.json", cancellationToken);
        response.EnsureSuccessStatusCode();
        var storyDto = await response.Content.ReadFromJsonAsync<StoryDto>(cancellationToken);
        
        return storyDto?.ToStory() 
            ?? throw new InvalidOperationException("Cannot deserialize story");
    }

    private sealed record StoryDto(
        string? By,
        int Descendants,
        int Score,
        int Time,
        string? Title,
        string? Url
    )
    {
        public Story ToStory() => new(
            Title,
            Url,
            By,
            Time,
            Score,
            Descendants
        );
    }
}