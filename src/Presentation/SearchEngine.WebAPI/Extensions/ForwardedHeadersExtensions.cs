using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace SearchEngine.WebAPI.Extensions;

/// <summary>
/// Konfigurasi Forwarded Headers agar client IP asli terbaca dengan benar
/// ketika aplikasi berada di belakang reverse proxy / load balancer.
/// Aman secara default: hanya proxy yang dikonfigurasi eksplisit
/// (ForwardedHeaders:KnownProxies) yang dipercaya, sehingga header
/// X-Forwarded-* dari sumber tak tepercaya tidak dapat memalsukan client IP.
/// </summary>
public static class ForwardedHeadersExtensions
{
    public static IServiceCollection
        AddSearchEngineForwardedHeaders(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor
                | ForwardedHeaders.XForwardedProto;

            // Default aman: kosongkan daftar bawaan, hanya percayai proxy yang
            // dikonfigurasi. Tanpa proxy terpercaya, header forwarded diabaikan
            // (tidak bisa dipakai untuk spoofing client IP).
            options.KnownProxies.Clear();
            options.KnownIPNetworks.Clear();

            var knownProxies =
                configuration
                    .GetSection("ForwardedHeaders:KnownProxies")
                    .Get<string[]>()
                ?? [];

            foreach (var proxy in knownProxies)
            {
                if (IPAddress.TryParse(proxy, out var ip))
                {
                    options.KnownProxies.Add(ip);
                }
            }
        });

        return services;
    }

    public static IApplicationBuilder
        UseSearchEngineForwardedHeaders(
            this IApplicationBuilder app)
    {
        app.UseForwardedHeaders();

        return app;
    }
}
