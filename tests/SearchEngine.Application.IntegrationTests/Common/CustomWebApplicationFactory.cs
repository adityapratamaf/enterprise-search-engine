using System.Net;
using SearchEngine.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SearchEngine.Application.IntegrationTests.Common;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    // IP tetap yang distempel pada setiap request test sehingga
    // audit CreatedByIp / RevokedByIp dapat diverifikasi. TestServer
    // secara default tidak mengisi RemoteIpAddress.
    public const string TestClientIp = "203.0.113.10";

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<
                IStartupFilter,
                RemoteIpStartupFilter>();
        });
    }

    protected override IHost CreateHost(
        IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // Reset lockout akun admin setiap kali test host dibangun.
        //
        // Integration test berjalan terhadap database yang sama (persisten),
        // sedangkan skenario negatif (login dengan password salah)
        // menaikkan AccessFailedCount karena lockoutOnFailure = true.
        // Tanpa reset, nilai ini terakumulasi antar-run hingga mencapai
        // MaxFailedAccessAttempts (5) dan mengunci akun admin selama 15 menit,
        // sehingga login valid berikutnya balas 401 (Unauthorized).
        ResetAdminLockoutAsync(host.Services)
            .GetAwaiter()
            .GetResult();

        return host;
    }

    private static async Task ResetAdminLockoutAsync(
        IServiceProvider services)
    {
        using var scope =
            services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

        var admin =
            await userManager.FindByEmailAsync(
                "admin@searchengine.local");

        if (admin is null)
        {
            return;
        }

        await userManager.SetLockoutEndDateAsync(
            admin,
            null);

        await userManager.ResetAccessFailedCountAsync(
            admin);
    }

    private sealed class RemoteIpStartupFilter
        : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(
            Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.Use(async (context, nextMiddleware) =>
                {
                    context.Connection.RemoteIpAddress =
                        IPAddress.Parse(TestClientIp);

                    await nextMiddleware();
                });

                next(app);
            };
        }
    }
}
