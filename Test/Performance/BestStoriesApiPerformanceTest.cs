using FluentAssertions;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using Xunit.Abstractions;

namespace Test.Performance;

public class BestStoriesApiPerformanceTest(ITestOutputHelper outputHelper)
{
    [Fact]
    public void GetBestStoriesShouldHandleAtLeast100RequestsPerSecond()
    {
        // given
        var apiUrl = Environment.GetEnvironmentVariable("API_URL") 
                  ?? throw new Exception("API_URL env var is not set");
        const int expectedRequestsPerSecond = 100;
        const int durationSeconds = 5;
        using var httpClient = new HttpClient();

        // when
        var stats = NBomberRunner
            .RegisterScenarios(
                Scenario.Create("n=100", _ => GetBestStories(n: 100, apiUrl, httpClient))
                    .WithWarmUpDuration(TimeSpan.FromSeconds(3))
                    .WithLoadSimulations(
                        Simulation.Inject(rate: 100,
                            interval: TimeSpan.FromSeconds(1),
                            during: TimeSpan.FromMinutes(3))
                    )
            ).Run();
        var scenarioStats = stats.GetScenarioStats("n=100");

        // then
        outputHelper.WriteLine($"OK: {scenarioStats.AllOkCount}, FAILED: {scenarioStats.AllFailCount}");
        scenarioStats.AllOkCount.Should()
            .BeGreaterThanOrEqualTo(durationSeconds * expectedRequestsPerSecond);
    }

    private static async Task<IResponse> GetBestStories(int n, string url, HttpClient httpClient)
    {
        try
        {
            var request = Http.CreateRequest("GET", $"{url}/beststories?n={n}");

            return await Http.Send(httpClient, request);
        }
        catch
        {
            return Response.Fail();
        }
    }
}