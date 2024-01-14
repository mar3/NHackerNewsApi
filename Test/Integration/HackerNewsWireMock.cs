using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using WireMock;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Types;
using WireMock.Util;

namespace Test.Integration;

public static class HackerNewsWireMock
{
    public static WireMockServer Default()
    {
        var wireMockServer = WireMockServer.Start();
        wireMockServer
            .Given(Request.Create().WithPath("/v0/beststories.json").UsingGet())
            .RespondWith(Response.Create()
                .WithBody(JsonSerializer.Serialize(Enumerable.Range(1, 100))));
        wireMockServer
            .Given(Request.Create().WithPath("/v0/item/*.json").UsingGet())
            .RespondWith(Response.Create().WithCallback(request =>
            {
                var id = int.Parse(Regex.Match(request.Path, "/v0/item/(?<id>\\d+).json", RegexOptions.Compiled)
                    .Groups["id"].Value);
                return new ResponseMessage
                {
                    BodyDestination = BodyDestinationFormat.SameAsSource,
                    BodyData = new BodyData
                    {
                        Encoding = Encoding.UTF8,
                        DetectedBodyType = BodyType.String,
                        BodyAsString = $$"""
                                         {
                                         "id": {{id}},
                                         "by": "ismaildonmez",
                                         "descendants": {{id*2}},
                                         "kids": [],
                                         "score": 1757,
                                         "time": 1570887781,
                                         "title": "some title for {{id}}",
                                         "type": "story",
                                         "url": "https://url.local/{{id}}"
                                         }
                                         """,
                    }
                };
            }));

        return wireMockServer;
    }
}