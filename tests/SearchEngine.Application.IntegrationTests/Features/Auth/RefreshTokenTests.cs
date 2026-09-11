using SearchEngine.Application.IntegrationTests.Common;
using FluentAssertions;
using System.Net;
using System.Text;
using System.Text.Json;

namespace SearchEngine.Application.IntegrationTests.Features.Auth;

/// <summary>
/// Integration tests untuk Refresh Token.
/// Memverifikasi proses perpanjangan access token
/// menggunakan refresh token yang valid maupun tidak valid.
/// </summary>
public class RefreshTokenTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RefreshTokenTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Memastikan endpoint POST /api/auth/refresh
    /// menghasilkan access token dan refresh token baru
    /// ketika refresh token masih valid.
    /// </summary>
    [Fact]
    public async Task Refresh_Should_Return_New_Tokens()
    {
        var auth =
            await AuthTestHelper
                .LoginAsync(_client);

        var payload = new
        {
            refreshToken =
                auth.Refresh
        };

        var json =
            JsonSerializer.Serialize(
                payload);

        var content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

        var response =
            await _client.PostAsync(
                "/api/auth/refresh",
                content);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Memastikan endpoint POST /api/auth/login
    /// mengembalikan HTTP 401 Unauthorized
    /// ketika email atau password tidak valid.
    /// </summary>
    [Fact]
    public async Task Login_Should_Return_401_When_Invalid_Credentials()
    {
        // Arrange

        var payload = new
        {
            email = "admin@searchengine.local",
            password = "PasswordSalah"
        };

        var json =
            JsonSerializer.Serialize(payload);

        var content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

        // Act
        var response =
            await _client.PostAsync(
                "/api/auth/login",
                content);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }
}
