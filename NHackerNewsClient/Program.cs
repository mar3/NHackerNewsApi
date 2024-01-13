using NHackerNewsClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<IHackerNewsClient, HttpHackerNewsClient>(client =>
{
    client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/beststories", async (int n, IHackerNewsClient client) =>
    {
        var storyIds = await client.GetBestStoryIdsAsync();
        var stories = await Task.WhenAll(storyIds.Take(n).Select(client.GetStoryAsync));
        return stories;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();