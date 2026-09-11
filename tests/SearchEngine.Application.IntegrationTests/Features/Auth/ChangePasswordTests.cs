using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SearchEngine.Application.IntegrationTests.Common;
using SearchEngine.Infrastructure.Identity.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace SearchEngine.Application.IntegrationTests.Features.Auth;

/// <summary>
/// Integration tests untuk endpoint Change Password
/// (PUT /api/auth/change-password).
/// </summary>
public class ChangePasswordTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private const string AdminEmail =
        "admin@searchengine.local";

    private const string AdminPassword =
        "Admin123!";

    private readonly CustomWebApplicationFactory _factory;

    private readonly HttpClient _client;

    public ChangePasswordTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Request tanpa JWT access token harus ditolak dengan 401.
    /// </summary>
    [Fact]
    public async Task ChangePassword_Should_Return_401_When_Unauthorized()
    {
        var response =
            await PutChangePasswordAsync(new
            {
                currentPassword = AdminPassword,
                newPassword = "NewPassword123!",
                confirmPassword = "NewPassword123!"
            });

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Skenario negatif (satu login dipakai bersama untuk menghormati
    /// rate limit login): current password salah, new password sama
    /// dengan current password, dan konfirmasi password tidak cocok.
    /// Tidak satu pun yang benar-benar mengubah password.
    /// </summary>
    [Fact]
    public async Task ChangePassword_Should_Reject_Invalid_Requests()
    {
        await AuthenticateAsAdminAsync();

        // Current password salah → gagal bisnis (200, success = false).
        var wrongCurrent =
            await PutChangePasswordAsync(new
            {
                currentPassword = "WrongPassword123!",
                newPassword = "NewPassword123!",
                confirmPassword = "NewPassword123!"
            });

        wrongCurrent.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        (await ReadResultAsync(wrongCurrent))
            .Success
            .Should()
            .BeFalse();

        // New password == current password → gagal bisnis.
        var sameAsCurrent =
            await PutChangePasswordAsync(new
            {
                currentPassword = AdminPassword,
                newPassword = AdminPassword,
                confirmPassword = AdminPassword
            });

        sameAsCurrent.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        (await ReadResultAsync(sameAsCurrent))
            .Success
            .Should()
            .BeFalse();

        // Konfirmasi password tidak cocok → gagal validasi (400).
        var mismatch =
            await PutChangePasswordAsync(new
            {
                currentPassword = AdminPassword,
                newPassword = "NewPassword123!",
                confirmPassword = "Different123!"
            });

        mismatch.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Perubahan password yang valid harus berhasil.
    /// Password admin dikembalikan setelah test agar test lain tetap login.
    /// </summary>
    [Fact]
    public async Task ChangePassword_Should_Succeed_With_Valid_Request()
    {
        await AuthenticateAsAdminAsync();

        const string newPassword = "NewPassword123!";

        try
        {
            var response =
                await PutChangePasswordAsync(new
                {
                    currentPassword = AdminPassword,
                    newPassword,
                    confirmPassword = newPassword
                });

            response.StatusCode
                .Should()
                .Be(HttpStatusCode.OK);

            var result =
                await ReadResultAsync(response);

            result.Success
                .Should()
                .BeTrue();

            result.Message
                .Should()
                .Be("Password changed successfully.");
        }
        finally
        {
            await ResetAdminPasswordAsync(AdminPassword);
        }
    }

    private async Task AuthenticateAsAdminAsync()
    {
        var auth =
            await AuthTestHelper.LoginAsync(_client);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                auth.Access);
    }

    private Task<HttpResponseMessage>
        PutChangePasswordAsync(
            object payload)
    {
        var json =
            JsonSerializer.Serialize(payload);

        var content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

        return _client.PutAsync(
            "/api/auth/change-password",
            content);
    }

    private static async Task<ApiResponse<string>>
        ReadResultAsync(
            HttpResponseMessage response)
    {
        var body =
            await response.Content
                .ReadAsStringAsync();

        return JsonSerializer.Deserialize<
            ApiResponse<string>>(
                body,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;
    }

    private async Task ResetAdminPasswordAsync(
        string password)
    {
        using var scope =
            _factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

        var admin =
            await userManager.FindByEmailAsync(
                AdminEmail);

        if (admin is null)
        {
            return;
        }

        var token =
            await userManager
                .GeneratePasswordResetTokenAsync(admin);

        await userManager.ResetPasswordAsync(
            admin,
            token,
            password);
    }
}
