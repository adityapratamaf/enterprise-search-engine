using System.Security.Claims;
using System.Text;
using SearchEngine.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace SearchEngine.WebAPI.Middleware;

/// <summary>
/// Memproteksi dashboard operasional (Hangfire &amp; Health Checks UI) dengan
/// HTTP Basic Auth. Popup username/password bawaan browser divalidasi ke akun
/// ASP.NET Identity, dan hanya user ber-role SuperAdmin yang diizinkan.
/// Pada keberhasilan, principal SuperAdmin di-set ke context.User sehingga
/// filter otorisasi bawaan Hangfire ikut lolos.
/// </summary>
public class DashboardBasicAuthMiddleware
{
    private const string Realm = "SearchEngine Admin";

    private const string RequiredRole = "SuperAdmin";

    // Endpoint data (/healthcheck-json) & aset statis (/healthcheck-resources)
    // sengaja tidak diproteksi: yang pertama untuk probe monitoring, yang kedua
    // hanya berkas JS/CSS dashboard.
    private static readonly string[] ProtectedPaths =
    [
        "/hangfire",
        "/healthcheck-ui",
        "/healthcheck-api"
    ];

    private readonly RequestDelegate _next;

    public DashboardBasicAuthMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context)
    {
        var path =
            context.Request.Path.Value
            ?? string.Empty;

        var isProtected =
            ProtectedPaths.Any(p =>
                path.StartsWith(
                    p,
                    StringComparison.OrdinalIgnoreCase));

        if (!isProtected)
        {
            await _next(context);
            return;
        }

        var principal =
            await AuthenticateAsync(context);

        if (principal is not null)
        {
            context.User = principal;
            await _next(context);
            return;
        }

        context.Response.StatusCode =
            StatusCodes.Status401Unauthorized;

        context.Response.Headers.WWWAuthenticate =
            $"Basic realm=\"{Realm}\", charset=\"UTF-8\"";
    }

    private static async Task<ClaimsPrincipal?>
        AuthenticateAsync(
            HttpContext context)
    {
        var header =
            context.Request.Headers.Authorization
                .ToString();

        if (string.IsNullOrWhiteSpace(header)
            || !header.StartsWith(
                "Basic ",
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        string decoded;

        try
        {
            var encoded =
                header["Basic ".Length..].Trim();

            decoded =
                Encoding.UTF8.GetString(
                    Convert.FromBase64String(encoded));
        }
        catch
        {
            return null;
        }

        var separator = decoded.IndexOf(':');

        if (separator < 0)
        {
            return null;
        }

        var username = decoded[..separator];
        var password = decoded[(separator + 1)..];

        var userManager =
            context.RequestServices
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

        var user =
            await userManager.FindByNameAsync(username)
            ?? await userManager.FindByEmailAsync(username);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        if (!await userManager.CheckPasswordAsync(
                user,
                password))
        {
            return null;
        }

        if (!await userManager.IsInRoleAsync(
                user,
                RequiredRole))
        {
            return null;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? username),
            new(ClaimTypes.Role, RequiredRole)
        };

        var identity =
            new ClaimsIdentity(
                claims,
                "Basic");

        return new ClaimsPrincipal(identity);
    }
}
