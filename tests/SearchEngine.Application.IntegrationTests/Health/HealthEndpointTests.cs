using System.Net;
using SearchEngine.Application.IntegrationTests.Common;
using FluentAssertions;

namespace SearchEngine.Application.IntegrationTests.Health;

/// <summary>
/// A-008: liveness and readiness probes. Both are unauthenticated and return
/// 200 when the process is alive and its dependencies are reachable.
/// </summary>
public class HealthEndpointTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Liveness_Should_Return_200()
    {
        var response =
            await _client.GetAsync("/health/live");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Readiness_Should_Return_200_When_Dependencies_Available()
    {
        var response =
            await _client.GetAsync("/health/ready");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }
}
