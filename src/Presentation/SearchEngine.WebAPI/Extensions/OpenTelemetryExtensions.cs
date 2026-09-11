using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter;

namespace SearchEngine.WebAPI.Extensions;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection
        AddSearchEngineOpenTelemetry(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        var serviceName =
            configuration["OpenTelemetry:ServiceName"]
            ?? "SearchEngine.WebAPI";

        var otlpEndpoint =
            configuration["OpenTelemetry:OtlpEndpoint"];

        var resourceBuilder =
            ResourceBuilder
                .CreateDefault()
                .AddService(serviceName);

        services
            .AddOpenTelemetry()

            // ---- Traces ----
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter();

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                        options.Protocol = OtlpExportProtocol.Grpc;
                    });
                }
            })

            // ---- Metrics ----
            // Uses the built-in .NET/ASP.NET Core meters (no extra packages):
            // request throughput/latency, Kestrel connections, rate limiting,
            // outbound HTTP, and .NET runtime (GC / heap / thread pool).
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resourceBuilder)
                    .AddMeter("Microsoft.AspNetCore.Hosting")
                    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                    .AddMeter("Microsoft.AspNetCore.RateLimiting")
                    .AddMeter("System.Net.Http")
                    .AddMeter("System.Runtime")
                    .AddMeter("Microsoft.EntityFrameworkCore");

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    metrics.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                        options.Protocol = OtlpExportProtocol.Grpc;
                    });
                }
            });

        return services;
    }
}
