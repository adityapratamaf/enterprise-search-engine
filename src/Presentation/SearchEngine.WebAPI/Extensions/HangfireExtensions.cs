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
