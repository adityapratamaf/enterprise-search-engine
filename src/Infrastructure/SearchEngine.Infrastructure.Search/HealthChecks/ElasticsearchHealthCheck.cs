using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using HttpMethod = Elastic.Transport.HttpMethod;

namespace SearchEngine.Infrastructure.Search.HealthChecks;

/// <summary>
/// Memeriksa kesehatan cluster Elasticsearch.
///
/// Status "yellow" dilaporkan sebagai sehat, bukan terdegradasi: pada
/// penyebaran satu node, replika tidak mungkin ditempatkan sehingga cluster
/// memang selalu kuning. Yang benar-benar menandakan masalah adalah "red",
/// yaitu ketika ada shard utama yang tidak tersedia.
/// </summary>
public sealed class ElasticsearchHealthCheck
    : IHealthCheck
{
    private readonly ElasticsearchClient _client;

    public ElasticsearchHealthCheck(
        ElasticsearchClient client)
    {
        _client = client;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint =
                new EndpointPath(
                    HttpMethod.GET,
                    "_cluster/health");

            var respons =
                await _client.Transport
                    .RequestAsync<StringResponse>(
                        endpoint,
                        null,
                        null,
                        null,
                        cancellationToken);

            if (!respons.ApiCallDetails.HasSuccessfulStatusCode)
            {
                return HealthCheckResult.Unhealthy(
                    "Elasticsearch membalas HTTP "
                    + respons.ApiCallDetails.HttpStatusCode);
            }

            var body = respons.Body ?? string.Empty;

            var status =
                body.Contains("\"status\":\"red\"",
                    StringComparison.OrdinalIgnoreCase)
                    ? "red"
                    : body.Contains("\"status\":\"yellow\"",
                        StringComparison.OrdinalIgnoreCase)
                        ? "yellow"
                        : "green";

            return status == "red"
                ? HealthCheckResult.Unhealthy(
                    "Status cluster Elasticsearch: red")
                : HealthCheckResult.Healthy(
                    $"Status cluster Elasticsearch: {status}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Elasticsearch tidak dapat dihubungi.",
                ex);
        }
    }
}
