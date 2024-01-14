using System.Runtime.CompilerServices;

namespace NHackerNewsClient;

public sealed class BestStoriesService(IHackerNewsClient client)
{
    public async IAsyncEnumerable<Story> GetBestStoriesAsync(int limit, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var storiesTasks = (await client.GetBestStoryIdsAsync(cancellationToken))
            .Take(limit)
            .Select(id => client.GetStoryAsync(id, cancellationToken))
            .ToList();
        
        foreach (var storyTask in storiesTasks)
        {
            yield return await storyTask;
        }
    }
}