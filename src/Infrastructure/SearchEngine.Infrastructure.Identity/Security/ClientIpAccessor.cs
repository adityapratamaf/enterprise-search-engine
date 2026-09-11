using Microsoft.AspNetCore.Http;

namespace SearchEngine.Infrastructure.Identity.Security;

/// <summary>
/// Reusable helper untuk mengambil IP address pemanggil secara null-safe.
/// Menghormati reverse proxy header apabila ForwardedHeaders middleware
/// sudah dikonfigurasi (nilai akan tercermin pada RemoteIpAddress).
/// Jika HttpContext tidak tersedia (Integration Test / Background Job),
/// mengembalikan null tanpa melempar exception.
/// </summary>
public static class ClientIpAccessor
{
    public static string? GetIpAddress(
        IHttpContextAccessor httpContextAccessor)
    {
        return httpContextAccessor
            .HttpContext?
            .Connection
            .RemoteIpAddress?
            .ToString();
    }
}
