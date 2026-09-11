using System.Threading.RateLimiting;
using SearchEngine.Application.Common.Models;
using SearchEngine.WebAPI.Options;
using Microsoft.AspNetCore.RateLimiting;

namespace SearchEngine.WebAPI.Extensions;

public static class RateLimiterExtensions
{
    public static IServiceCollection
        AddSearchEngineRateLimiter(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        services.AddOptions<RateLimitingOptions>()
            .Bind(configuration.GetSection("RateLimiting"))
            .Validate(
                options =>
                    IsValid(options.Login)
                    && IsValid(options.Refresh)
                    && IsValid(options.Public)
                    && IsValid(options.Api),
                "Rate limiting PermitLimit and WindowMinutes must be greater than zero.")
            .ValidateOnStart();

        var settings =
            configuration
                .GetSection("RateLimiting")
                .Get<RateLimitingOptions>()
            ?? new RateLimitingOptions();

        services.AddRateLimiter(options =>
        {
            // =====================================================
            // GLOBAL RESPONSE
            // =====================================================

            options.OnRejected = async (
                context,
                cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode =
                    StatusCodes.Status429TooManyRequests;

                context.HttpContext.Response.ContentType =
                    "application/json";

                var response =
                    Result<object>.Failure(
                        "Too many requests.");

                await context.HttpContext.Response
                    .WriteAsJsonAsync(
                        response,
                        cancellationToken);
            };

            // =====================================================
            // LOGIN
            // =====================================================

            AddFixedWindowPolicy(
                options,
                "login",
                settings.Login);

            // =====================================================
            // REFRESH
            // =====================================================

            AddFixedWindowPolicy(
                options,
                "refresh",
                settings.Refresh);

            // =====================================================
            // PUBLIC
            // =====================================================

            AddFixedWindowPolicy(
                options,
                "public",
                settings.Public);

            // =====================================================
            // API
            // =====================================================

            AddFixedWindowPolicy(
                options,
                "api",
                settings.Api);
        });

        return services;
    }

    public static IApplicationBuilder
        UseSearchEngineRateLimiter(
            this IApplicationBuilder app)
    {
        app.UseRateLimiter();

        return app;
    }

    private static void AddFixedWindowPolicy(
        RateLimiterOptions options,
        string policyName,
        RateLimitPolicyOptions setting)
    {
        options.AddPolicy(
            policyName,
            context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey:
                        context.Connection
                            .RemoteIpAddress?
                            .ToString()
                        ?? "unknown",

                    factory: _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit =
                                setting.PermitLimit,

                            Window =
                                TimeSpan.FromMinutes(
                                    setting.WindowMinutes),

                            QueueLimit = 0,

                            QueueProcessingOrder =
                                QueueProcessingOrder
                                    .OldestFirst
                        }));
    }

    private static bool IsValid(
        RateLimitPolicyOptions options)
    {
        return options.PermitLimit > 0
               && options.WindowMinutes > 0;
    }
}
