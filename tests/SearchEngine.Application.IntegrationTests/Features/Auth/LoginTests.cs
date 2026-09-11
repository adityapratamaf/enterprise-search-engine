using SearchEngine.Application.IntegrationTests.Common;
using FluentAssertions;
using System.Net;
using System.Text;
using System.Text.Json;


namespace SearchEngine.Application.IntegrationTests.Features.Auth;

/// <summary>
/// Integration tests untuk Authentication Login.
/// Memverifikasi proses login berhasil
/// maupun gagal berdasarkan kredensial pengguna.
/// </summary>
public class LoginTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LoginTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Memastikan endpoint POST /api/auth/login
    /// berhasil menghasilkan access token
    /// dan refresh token ketika kredensial valid.
    /// </summary>
    [Fact]
    public async Task Login_Should_Return_Tokens()
    {
        var auth =
            await AuthTestHelper
                .LoginAsync(_client);

        auth.Access
            .Should()
            .NotBeNullOrWhiteSpace();

        auth.Refresh
            .Should()
            .NotBeNullOrWhiteSpace();
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
