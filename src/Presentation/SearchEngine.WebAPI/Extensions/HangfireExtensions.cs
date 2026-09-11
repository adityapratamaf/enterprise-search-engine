using SearchEngine.WebAPI.Jobs;

using Hangfire;
using Hangfire.Dashboard;

namespace SearchEngine.WebAPI.Extensions;

public static class HangfireExtensions
{
    public static IServiceCollection
        AddSearchEngineHangfire(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        services.AddHangfire(config =>
        {
            config.UseSqlServerStorage(
                configuration.GetConnectionString(
                    "HangfireConnection"));
        });

        services.AddHangfireServer();

        return services;
    }

    public static IApplicationBuilder
        UseSearchEngineHangfire(
            this IApplicationBuilder app)
    {
        var environment =
            app.ApplicationServices
                .GetRequiredService<IWebHostEnvironment>();

        // Indexing ulang terjadwal sebagai jaring pengaman. Perubahan data
        // yang tidak lewat aplikasi — mis. UPDATE langsung ke SQL Server —
        // tidak akan pernah memberi tahu Elasticsearch, sehingga membangun
        // ulang secara berkala adalah satu-satunya cara menjamin penyimpangan
        // tidak menumpuk diam-diam.
        // Memakai IRecurringJobManager dari container, bukan kelas statis
        // RecurringJob: yang statis bergantung pada JobStorage.Current global
        // yang belum tentu terpasang saat pipeline dibangun.
        app.ApplicationServices
            .GetRequiredService<IRecurringJobManager>()
            .AddOrUpdate<SpbuReindexJob>(
                "spbu-reindex-harian",
                job => job.RunAsync(CancellationToken.None),
                "0 2 * * *",
                new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.Utc
                });

        if (environment.IsDevelopment())
        {
            // Bisa Diakses Saat Development
            app.UseHangfireDashboard("/hangfire");
        }
        else
        {
            // Bisa Diakses SuperAdmin Saat Production
            app.UseHangfireDashboard(
                "/hangfire",
                new DashboardOptions
                {
                    Authorization =
                    [
                        new HangfireDashboardAuthorizationFilter()
                    ]
                });
        }

        return app;
    }

    private sealed class HangfireDashboardAuthorizationFilter
        : IDashboardAuthorizationFilter
    {
        public bool Authorize(
            DashboardContext context)
        {
            var httpContext =
                context.GetHttpContext();

            return httpContext.User.Identity?.IsAuthenticated == true
                && httpContext.User.IsInRole("SuperAdmin");
        }
    }
}
