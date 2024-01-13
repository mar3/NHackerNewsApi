using System.Text.Json;

namespace NHackerNewsClient;

public interface IHackerNewsClient
{
    Task<IEnumerable<int>> GetBestStoryIdsAsync();
    Task<Story> GetStoryAsync(int id);
}

public sealed class HttpHackerNewsClient(HttpClient httpClient) : IHackerNewsClient
{
    public async Task<IEnumerable<int>> GetBestStoryIdsAsync()
    {
        var response = await httpClient.GetAsync("beststories.json");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        
        return JsonSerializer.Deserialize<IEnumerable<int>>(content)
            ?? throw new InvalidOperationException("Cannot deserialize best stories");
    }

    public async Task<Story> GetStoryAsync(int id)
    {
        var response = await httpClient.GetAsync($"item/{id}.json");
        response.EnsureSuccessStatusCode();
        var storyDto = await response.Content.ReadFromJsonAsync<StoryDto>();
        
        return storyDto?.ToStory() 
            ?? throw new InvalidOperationException("Cannot deserialize story");
    }

    private sealed record StoryDto(
        string? By,
        int Descendants,
        int Id,
        int Score,
        int Time,
        string? Title,
        string? Type,
        string? Url
    )
    {
        public Story ToStory() => new(
            Id,
            By,
            Descendants,
            Score,
            Time,
            Title,
            Url
        );
    }
}

public sealed record Story(
    int Id,
    string? PostedBy,
    int CommentCount,
    int Score,
    int Time,
    string? Title,
    string? Url
);