using SearchEngine.Application.IntegrationTests.Common;
using FluentAssertions;
using System.Net;

namespace SearchEngine.Application.IntegrationTests.Health;

public class HealthTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheckJson_Should_Return_200()
    {
        var response =
            await _client.GetAsync(
                "/healthcheck-json");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }
}
