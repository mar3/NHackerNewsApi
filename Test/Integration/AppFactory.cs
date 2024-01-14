using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NHackerNewsClient;
using WireMock.Server;

namespace Test.Integration;

public class AppFactory : WebApplicationFactory<IProgramMarker>
{
    public WireMockServer WireMockServer { get; } = HackerNewsWireMock.Default();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("HackerNews__BaseUrl", WireMockServer.Url);
        Environment.SetEnvironmentVariable("HackerNews__CacheDurationMilliseconds", "60000");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(IHackerNewsClient));
            services.AddTransient<IHackerNewsClient>(provider => provider.GetRequiredService<HttpHackerNewsClient>());
        });
    }
}