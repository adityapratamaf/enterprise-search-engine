using SearchEngine.WebAPI.HealthChecks;
using SearchEngine.Infrastructure.Search.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SearchEngine.WebAPI.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection
        AddSearchEngineHealthChecks(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        var smtpHost =
            configuration["SMTP:Host"]
            ?? "localhost";

        var smtpPort =
            int.TryParse(
                configuration["SMTP:Port"],
                out var parsedPort)
                ? parsedPort
                : 587;

        services
            .AddHealthChecks()

            // ---- Liveness: process is alive (no external dependencies) ----
            .AddCheck(
                "self",
                () => HealthCheckResult.Healthy("Alive"),
                tags: ["live"])

            // ---- Databases (critical -> Unhealthy) ----
            .AddSqlServer(
                configuration.GetConnectionString(
                    "BusinessConnection")!,
                name: "Application Database: SQL Server",
                tags: ["Database"])

            .AddSqlServer(
                configuration.GetConnectionString(
                    "IdentityConnection")!,
                name: "Identity Database: SQL Server",
                tags: ["Database"])

            .AddSqlServer(
                configuration.GetConnectionString(
                    "HangfireConnection")!,
                name: "Background Job Database: SQL Server (Hangfire)",
                tags: ["Database"])

            // ---- Background Job Service (non-critical -> Degraded) ----
            .AddHangfire(
                setup =>
                    setup.MinimumAvailableServers = 1,
                name: "Background Job Service: Hangfire",
                failureStatus: HealthStatus.Degraded,
                tags: ["Background Job"])

            // ---- Email/SMTP (non-critical -> Degraded). Custom TCP-reachability
            //      check agar tidak menarik dependency SSH.NET (rentan) dari
            //      package Network. ----
            .AddTypeActivatedCheck<SmtpHealthCheck>(
                name: "Email Service: SMTP",
                failureStatus: HealthStatus.Degraded,
                tags: ["Email"],
                timeout: TimeSpan.FromSeconds(5),
                args: [smtpHost, smtpPort])

            // ---- File Storage (critical -> Unhealthy) ----
            .AddCheck<FileStorageHealthCheck>(
                "File Storage Service: File System",
                tags: ["File Storage"])

            // ---- Search engine (critical -> Unhealthy). Seluruh pencarian
            //      dilayani dari sini; bila mati, fitur utama aplikasi hilang. ----
            .AddCheck<ElasticsearchHealthCheck>(
                "Search Engine: Elasticsearch",
                tags: ["Search"]);

        // URL yang di-poll collector. Wajib absolut (collector jalan di
        // background tanpa konteks request). Bisa dioverride via konfigurasi
        // untuk environment non-lokal.
        var pollEndpoint =
            configuration["HealthChecksUI:HealthCheckEndpoint"]
            ?? "http://127.0.0.1:5152/healthcheck-json";

        services
            .AddHealthChecksUI(setup =>
            {
                setup.AddHealthCheckEndpoint(
                    "SearchEngine API",
                    pollEndpoint);

                setup.SetEvaluationTimeInSeconds(15);

                setup.MaximumHistoryEntriesPerEndpoint(50);
            })
            .AddSqlServerStorage(
                configuration.GetConnectionString(
                    "HangfireConnection")!);

        return services;
    }

    public static WebApplication
        UseSearchEngineHealthChecks(
            this WebApplication app)
    {
        // Endpoint data (JSON format-UI) yang di-poll dashboard.
        // Juga dipakai monitoring & Docker HEALTHCHECK.
        app.UseHealthChecks(
            "/healthcheck-json",
            new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter =
                    UIResponseWriter
                        .WriteHealthCheckUIResponse
            });

        // Liveness probe: only the "self" check (no external dependencies).
        // A transient database outage must NOT make a live process appear dead.
        app.UseHealthChecks(
            "/health/live",
            new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("live")
            });

        // Readiness probe: all dependency checks (database, storage, etc.).
        // Returns 503 when a critical dependency is unavailable.
        app.UseHealthChecks(
            "/health/ready",
            new HealthCheckOptions
            {
                Predicate = check => !check.Tags.Contains("live")
            });

        // Dashboard UI. URL yang dibuka pengguna: /healthcheck-ui
        app.MapHealthChecksUI(options =>
        {
            options.UIPath = "/healthcheck-ui";
            options.ApiPath = "/healthcheck-api";
            options.ResourcesPath = "/healthcheck-resources";
        });

        return app;
    }
}
