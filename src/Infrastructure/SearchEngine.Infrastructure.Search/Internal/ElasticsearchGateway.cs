using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

using HttpMethod = Elastic.Transport.HttpMethod;

namespace SearchEngine.Infrastructure.Search.Internal;

/// <summary>
/// Pengirim permintaan mentah ke Elasticsearch.
///
/// Seluruh interaksi dengan Elasticsearch di project ini melewati satu pintu
/// ini dalam bentuk JSON apa adanya, bukan lewat API berjenjang klien.
/// Alasannya sama untuk indexing maupun pencarian: bentuk kueri yang dipakai
/// adalah bentuk yang sudah diuji langsung di Kibana Dev Tools, sehingga
/// tidak ada risiko perilakunya berubah karena kesalahan penerjemahan.
/// </summary>
public sealed class ElasticsearchGateway
{
    public const string JsonContentType = "application/json";

    /// <summary>Endpoint _bulk menolak application/json; harus NDJSON.</summary>
    public const string NdJsonContentType = "application/x-ndjson";

    private readonly ElasticsearchClient _client;

    public ElasticsearchGateway(
        ElasticsearchClient client)
    {
        _client = client;
    }

    /// <param name="terimaGagal">
    /// Bila true, tanggapan dengan status gagal dikembalikan apa adanya
    /// alih-alih melempar. Dipakai untuk permintaan yang "tidak ditemukan"
    /// -nya merupakan hasil yang sah, mis. memeriksa keberadaan index.
    /// </param>
    public async Task<string> SendAsync(
        HttpMethod method,
        string path,
        string? body,
        string contentType = JsonContentType,
        CancellationToken cancellationToken = default,
        bool terimaGagal = false)
    {
        var endpoint =
            new EndpointPath(method, path);

        var konfigurasi =
            new RequestConfiguration
            {
                ContentType = contentType,
                Accept = JsonContentType
            };

        var respons =
            await _client.Transport
                .RequestAsync<StringResponse>(
                    endpoint,
                    body is null
                        ? null
                        : PostData.String(body),
                    null,
                    konfigurasi,
                    cancellationToken);

        if (respons.ApiCallDetails.HasSuccessfulStatusCode || terimaGagal)
        {
            return respons.Body ?? string.Empty;
        }

        throw new InvalidOperationException(
            $"Permintaan Elasticsearch gagal: {method} /{path} "
            + $"→ HTTP {respons.ApiCallDetails.HttpStatusCode}. "
            + $"Tanggapan: {respons.Body}");
    }
}
