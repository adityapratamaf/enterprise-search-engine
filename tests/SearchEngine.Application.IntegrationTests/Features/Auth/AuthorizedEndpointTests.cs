using SearchEngine.Application.IntegrationTests.Common;
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;

namespace SearchEngine.Application.IntegrationTests.Features.Auth;

public class AuthorizedEndpointTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthorizedEndpointTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Products_Should_Return_200_When_Authorized()
    {
        var auth =
            await AuthTestHelper
                .LoginAsync(_client);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                auth.Access);

        var response =
            await _client.GetAsync(
                "/api/products");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }
}
