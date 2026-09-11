using SearchEngine.WebAPI.Middleware;

namespace SearchEngine.WebAPI.Extensions;

public static class SecurityHeadersExtensions
{
    public static IApplicationBuilder
        UseSearchEngineSecurityHeaders(
            this IApplicationBuilder app)
    {
        app.UseMiddleware<SecurityHeadersMiddleware>();

        return app;
    }
}
