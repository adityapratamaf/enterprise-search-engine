using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SearchEngine.WebAPI.HealthChecks;

/// <summary>
/// Memverifikasi folder storage dapat ditulis &amp; dibaca aplikasi dengan
/// menulis lalu membaca berkas probe kecil, kemudian menghapusnya.
/// </summary>
public class FileStorageHealthCheck
    : IHealthCheck
{
    public async Task<HealthCheckResult>
        CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
    {
        var storagePath =
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "storage",
                "uploads");

        try
        {
            Directory.CreateDirectory(storagePath);

            var probeFile =
                Path.Combine(
                    storagePath,
                    $".healthcheck-{Guid.NewGuid():N}.tmp");

            await File.WriteAllTextAsync(
                probeFile,
                "ok",
                cancellationToken);

            await File.ReadAllTextAsync(
                probeFile,
                cancellationToken);

            File.Delete(probeFile);

            return HealthCheckResult.Healthy(
                $"Application has read and write permissions to {storagePath}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                $"Storage folder is not read/write accessible: {storagePath}",
                ex);
        }
    }
}
