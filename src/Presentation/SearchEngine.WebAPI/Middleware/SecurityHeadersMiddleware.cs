namespace SearchEngine.WebAPI.Middleware;

/// <summary>
/// Menambahkan security response headers standar untuk seluruh response.
/// Digunakan untuk meningkatkan keamanan Web API tanpa mengganggu
/// dashboard atau UI seperti Hangfire dan Scalar.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(
        HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;

            headers["X-Content-Type-Options"] =
                "nosniff";

            headers["X-Frame-Options"] =
                "DENY";

            headers["Referrer-Policy"] =
                "no-referrer";

            headers["Permissions-Policy"] =
                "geolocation=(), camera=(), microphone=(), payment=(), usb=()";

            headers["X-Permitted-Cross-Domain-Policies"] =
                "none";

            // Tidak menambahkan Content-Security-Policy (CSP).
            //
            // Template ini merupakan ASP.NET Core Web API yang
            // mengembalikan JSON, bukan HTML. CSP lebih relevan
            // untuk aplikasi MVC, Razor Pages, atau Blazor.
            //
            // Menghindari CSP di level middleware juga mencegah
            // konflik dengan Hangfire Dashboard, Scalar, Swagger UI,
            // maupun dashboard lain yang mungkin ditambahkan pengguna.

            headers.Remove("X-Powered-By");
            headers.Remove("Server");

            return Task.CompletedTask;
        });

        return _next(context);
    }
}
