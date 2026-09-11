using System.Net;
using System.Text;
using System.Text.Json;
using SearchEngine.Application.IntegrationTests.Common;
using FluentAssertions;

namespace SearchEngine.Application.IntegrationTests.Features.Auth;

/// <summary>
/// A-006: a refresh token is a single-use credential. Two concurrent refresh
/// attempts with the same token must result in exactly one success and one
/// failure (no double-consumption).
/// </summary>
public class RefreshTokenConcurrencyTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RefreshTokenConcurrencyTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Concurrent_Refresh_Should_Allow_Only_One_Success()
    {
        var auth =
            await AuthTestHelper.LoginAsync(_client);

        var payload =
            JsonSerializer.Serialize(
                new { refreshToken = auth.Refresh });

        StringContent Content() =>
            new(payload, Encoding.UTF8, "application/json");

        // Fire two refresh requests concurrently with the SAME token.
        var responses =
            await Task.WhenAll(
                _client.PostAsync("/api/auth/refresh", Content()),
                _client.PostAsync("/api/auth/refresh", Content()));

        var success =
            responses.Count(r => r.StatusCode == HttpStatusCode.OK);

        var failed =
            responses.Count(r => r.StatusCode == HttpStatusCode.Unauthorized);

        success.Should().Be(1);
        failed.Should().Be(1);
    }
}
