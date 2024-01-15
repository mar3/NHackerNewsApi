using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Diagnostics.ExceptionSummarization;
using NHackerNewsClient;
using ZiggyCreatures.Caching.Fusion;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<HackerNewsOptions>()
    .BindConfiguration(HackerNewsOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
var hackerNewsOptions = builder.Configuration
    .GetSection(HackerNewsOptions.SectionName)
    .Get<HackerNewsOptions>() ?? throw new Exception($"Cannot get {HackerNewsOptions.SectionName} from configuration");

builder.Services.AddHttpClient<HttpHackerNewsClient>(client =>
{
    client.BaseAddress = new Uri(hackerNewsOptions.BaseUrl);
}).AddStandardResilienceHandler(options =>
{
    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);
});

if (hackerNewsOptions.CacheDurationMilliseconds > 0)
{
    builder.Services
        .AddFusionCache()
        .WithDefaultEntryOptions(new FusionCacheEntryOptions {
            Duration = TimeSpan.FromMilliseconds(hackerNewsOptions.CacheDurationMilliseconds)
        });
    builder.Services.AddTransient<IHackerNewsClient>(provider => new CachedHackerNewsClient(
        provider.GetRequiredService<IFusionCache>(),
        provider.GetRequiredService<HttpHackerNewsClient>()
    ));
}
else
{
    builder.Services.AddTransient<IHackerNewsClient>(provider => 
        provider.GetRequiredService<HttpHackerNewsClient>());
}

builder.Services.AddTransient<BestStoriesService>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionSummarizer(static builder => builder.AddHttpProvider());
builder.Services.AddResilienceEnricher();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseRewriter(new RewriteOptions().AddRedirect("^$", "swagger"));
}

app.UseHttpsRedirection();

app.MapGet("/beststories", (BestStoriesService service, CancellationToken cancellationToken, int n = 5) =>
        n < 0 ? Results.BadRequest() : Results.Ok(service.GetBestStoriesAsync(n, cancellationToken)))
    .WithName("GetBestStories")
    .WithOpenApi();

app.Run();


public interface IProgramMarker; // for integration tests