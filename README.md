# NHackerNewsApi - best stories

This is a simple ASP.NET Core 8.0 Web API project that retrieves details of the first n "best stories" from the Hacker News API. 
It is a solution for recruitment task. 

## Questions

1. Does this app should always response actual data from API or it can be cached?
2. What if Hacker News API will be unavailable? Should return error or old data?
3. What if `n` is greater than best stories returned by Hacker News API? Should I return less stories than requested or return error?
4. How often this endpoint will be called? How many requests per second this app should handle? 
5. What response time is acceptable?
6. How many requests per second Hacker News API can handle?

## Assumptions

1. It is important to return actual data or only few seconds or less old, because `commentCount` can change often.
2. When Hacker News API is unavailable, it is ok to return error.
3. When `n` is greater than best stories returned by Hacker News API, it is ok to return less stories than requested.

## Decisions

Created endpoint `/beststories?n=5` returns current data, but it can be cached. I decided to call Hacker News API asynchronously and returning as IAsyncEnumerable to enable response streaming. For caching I use [FusionCache](https://github.com/ZiggyCreatures/FusionCache) package. It's support also distributed cache like redis so its's easy to add it in the future (especially important when horizontal scaling is needed).
Cache is enabled only when `HackerNews__CacheDurationMilliseconds` environment variable is set to value greater than 0. 
When cache expired and hacker news API is unavailable, api returns error. It can be easily changed to return old data (example enabling fail safe in fusion cache config).

> [!IMPORTANT]
> If data can be a little outdated and it is important to shorten response times, better solution is to use background service that will call the Hacker News API periodically and stores data.
> Then, it will be easier to add pagination and filtering, ETag, handle unavailable Hacker News API and response times will be much shorter, but when there is a few requests to this app, background service will be constantly calling Hacker News API.

## Features

- [x] Get first n best stories from Hacker News API
- [x] Support streaming
- [x] Cache results (when enabled - reduced calls to Hacker News API)
- [x] Resilience pipeline using [Polly](https://github.com/App-vNext/Polly) (retry, circuit breaker - can be easily changed) 

## How to run

Use docker compose to run application. It will build and start application.

```bash
docker comopse up -d
```

Application will be available at http://localhost:8080, swagger at http://localhost:8080/swagger/index.html.

## How to run tests

```bash
docker run --rm -v ${PWD}:/app -w /app mcr.microsoft.com/dotnet/sdk:8.0 dotnet test
```

Tests are written using [xUnit](https://xunit.net/), [WireMock.Net](https://github.com/WireMock-Net/WireMock.Net), [Verify](https://github.com/VerifyTests/Verify) and [FluentAssertions](https://fluentassertions.com/).

## Configuration

Environment variables (can be set in docker-compose.yml file):

| Name                                  | Description                                                                          | Example value                      |
|---------------------------------------|--------------------------------------------------------------------------------------|------------------------------------|
| HackerNews__BaseUrl                   | Hacker News API url                                                                  | https://hacker-news.firebaseio.com |
| HackerNews__CacheDurationMilliseconds | Cache duration in milliseconds. If value is less then or equal 0, cache is disabled. | 5000                               |

App can be configured also using appsettings.json file.

## Limitations

- Response time can be slow (more than 1 second) when `n` is big. It has to call Hacker News API for every story (n+1 requests).
- There is no any pagination or filtering
- When Hacker News API is unavailable and cache expired, it returns error. It can be easily changed to return old data (example enabling fail safe in fusion cache config).
- When `n` is greater than best stories returned by Hacker News API, it returns less stories than requested. It can be easily changed to return error.
- Only few tests are written.
- When error occurs, it could return more details about error.
- When caching is enabled caller can't know if data is from cache or not.
