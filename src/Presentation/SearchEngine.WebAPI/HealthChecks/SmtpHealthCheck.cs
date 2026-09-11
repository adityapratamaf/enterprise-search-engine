using System.Net.Sockets;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SearchEngine.WebAPI.HealthChecks;

/// <summary>
/// Memeriksa ketersediaan server SMTP dengan membuka koneksi TCP ke host/port.
/// Custom (tanpa package Network) agar tidak menarik dependency SSH.NET.
/// Kegagalan dilaporkan sebagai Degraded (email dependency non-kritis).
/// </summary>
public class SmtpHealthCheck
    : IHealthCheck
{
    private readonly string _host;

    private readonly int _port;

    public SmtpHealthCheck(
        string host,
        int port)
    {
        _host = host;
        _port = port;
    }

    public async Task<HealthCheckResult>
        CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new TcpClient();

            await client.ConnectAsync(
                _host,
                _port,
                cancellationToken);

            return HealthCheckResult.Healthy(
                $"SMTP reachable at {_host}:{_port}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded(
                $"SMTP not reachable at {_host}:{_port}",
                ex);
        }
    }
}
