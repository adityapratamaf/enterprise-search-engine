using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SearchEngine.Application.IntegrationTests.Common;
using SearchEngine.Infrastructure.Identity.Context;
using SearchEngine.Infrastructure.Identity.Security;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SearchEngine.Application.IntegrationTests.Features.Auth;

/// <summary>
/// Integration tests untuk hardening keamanan Refresh Token:
/// penyimpanan hash SHA256, rotasi token, deteksi reuse,
/// serta audit CreatedByIp / RevokedByIp.
/// </summary>
public class RefreshTokenSecurityTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    private readonly HttpClient _client;

    public RefreshTokenSecurityTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Login mengembalikan raw refresh token ke client,
    /// namun database hanya menyimpan hash SHA256-nya
    /// (plaintext tidak pernah dipersistensi) beserta CreatedByIp.
    /// </summary>
    [Fact]
    public async Task Login_Persists_Only_Sha256_Hash_And_Returns_Raw_Token()
    {
        var auth =
            await AuthTestHelper.LoginAsync(_client);

        auth.Refresh
            .Should()
            .NotBeNullOrWhiteSpace();

        var expectedHash =
            RefreshTokenHasher.Hash(auth.Refresh);

        using var scope =
            _factory.Services.CreateScope();

        var db =
            scope.ServiceProvider
                .GetRequiredService<
                    ApplicationIdentityDbContext>();

        var stored =
            await db.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.TokenHash == expectedHash);

        stored.Should().NotBeNull();

        stored!.TokenHash
            .Should()
            .Be(expectedHash);

        stored.TokenHash
            .Should()
            .NotBe(auth.Refresh);

        // Plaintext refresh token tidak boleh tersimpan di mana pun.
        var plaintextExists =
            await db.RefreshTokens
                .AsNoTracking()
                .AnyAsync(x =>
                    x.TokenHash == auth.Refresh);

        plaintextExists
            .Should()
            .BeFalse();

        stored.CreatedByIp
            .Should()
            .Be(CustomWebApplicationFactory.TestClientIp);
    }

    /// <summary>
    /// Refresh melakukan rotasi (token lama direvoke dan
    /// ReplacedByTokenHash diisi hash token baru), lalu penggunaan
    /// kembali token yang sudah direvoke terdeteksi sebagai reuse
    /// dan seluruh token aktif milik user ikut direvoke.
    /// </summary>
    [Fact]
    public async Task Refresh_Rotates_Token_And_Detects_Reuse()
    {
        var auth =
            await AuthTestHelper.LoginAsync(_client);

        var oldHash =
            RefreshTokenHasher.Hash(auth.Refresh);

        // ===== ROTATION =====
        var rotation =
            await PostRefreshAsync(auth.Refresh);

        rotation.Response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var newRaw =
            rotation.Body!.Data!.Refresh;

        newRaw.Should().NotBeNullOrWhiteSpace();

        newRaw.Should().NotBe(auth.Refresh);

        var newHash =
            RefreshTokenHasher.Hash(newRaw);

        using (var scope = _factory.Services.CreateScope())
        {
            var db =
                scope.ServiceProvider
                    .GetRequiredService<
                        ApplicationIdentityDbContext>();

            var oldToken =
                await db.RefreshTokens
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.TokenHash == oldHash);

            oldToken.Should().NotBeNull();

            oldToken!.IsRevoked
                .Should()
                .BeTrue();

            oldToken.ReplacedByTokenHash
                .Should()
                .Be(newHash);

            // Replacement token tidak boleh disimpan sebagai raw.
            oldToken.ReplacedByTokenHash
                .Should()
                .NotBe(newRaw);

            oldToken.RevokedByIp
                .Should()
                .Be(CustomWebApplicationFactory.TestClientIp);

            var newToken =
                await db.RefreshTokens
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.TokenHash == newHash);

            newToken.Should().NotBeNull();

            newToken!.IsRevoked
                .Should()
                .BeFalse();
        }

        // ===== REUSE DETECTION =====
        var reuse =
            await PostRefreshAsync(auth.Refresh);

        reuse.Response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);

        reuse.Body!.Message
            .Should()
            .Contain("reuse");

        using (var scope = _factory.Services.CreateScope())
        {
            var db =
                scope.ServiceProvider
                    .GetRequiredService<
                        ApplicationIdentityDbContext>();

            var newToken =
                await db.RefreshTokens
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.TokenHash == newHash);

            newToken.Should().NotBeNull();

            // Seluruh token aktif user direvoke saat reuse terdeteksi.
            newToken!.IsRevoked
                .Should()
                .BeTrue();

            newToken.RevokedByIp
                .Should()
                .Be(CustomWebApplicationFactory.TestClientIp);
        }
    }

    /// <summary>
    /// Logout merevoke refresh token aktif user dan mencatat RevokedByIp.
    /// </summary>
    [Fact]
    public async Task Logout_Revokes_Refresh_Token_And_Records_RevokedByIp()
    {
        var auth =
            await AuthTestHelper.LoginAsync(_client);

        var hash =
            RefreshTokenHasher.Hash(auth.Refresh);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/auth/logout");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                auth.Access);

        var response =
            await _client.SendAsync(request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        using var scope =
            _factory.Services.CreateScope();

        var db =
            scope.ServiceProvider
                .GetRequiredService<
                    ApplicationIdentityDbContext>();

        var token =
            await db.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.TokenHash == hash);

        token.Should().NotBeNull();

        token!.IsRevoked
            .Should()
            .BeTrue();

        token.RevokedByIp
            .Should()
            .Be(CustomWebApplicationFactory.TestClientIp);
    }

    private async Task<(
        HttpResponseMessage Response,
        ApiResponse<AuthResponse>? Body)>
        PostRefreshAsync(
            string refreshToken)
    {
        var json =
            JsonSerializer.Serialize(
                new { refreshToken });

        var content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

        var response =
            await _client.PostAsync(
                "/api/auth/refresh",
                content);

        var body =
            JsonSerializer.Deserialize<
                ApiResponse<AuthResponse>>(
                    await response.Content
                        .ReadAsStringAsync(),
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

        return (response, body);
    }
}
