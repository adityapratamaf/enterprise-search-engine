using System.Text.Json;
using System.Text.Json.Nodes;

using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Features.Search.DTOs;
using SearchEngine.Infrastructure.Search.Indexing;
using SearchEngine.Infrastructure.Search.Internal;

using Microsoft.Extensions.Options;

using HttpMethod = Elastic.Transport.HttpMethod;

namespace SearchEngine.Infrastructure.Search.Searching;

/// <summary>
/// Saran ketik-langsung memakai field <c>search_as_you_type</c>.
///
/// Berbeda dari pencarian utama, di sini TIDAK dipakai toleransi salah ketik.
/// Saat pengguna baru mengetik sebagian kata, setiap potongan pada dasarnya
/// memang "salah" — menambahkan fuzzy hanya membuat saran melebar ke
/// nama-nama yang tidak diharapkan. Yang dibutuhkan adalah pencocokan
/// awalan, dan itulah yang diberikan tipe <c>bool_prefix</c>.
/// </summary>
public sealed class SpbuSuggestionService
    : ISpbuSuggestionService
{
    private static readonly string[] FieldKetik =
    [
        "nama.ketik",
        "nama.ketik._2gram",
        "nama.ketik._3gram"
    ];

    private readonly ElasticsearchGateway _gateway;

    private readonly ElasticsearchOptions _options;

    public SpbuSuggestionService(
        ElasticsearchGateway gateway,
        IOptions<ElasticsearchOptions> options)
    {
        _gateway = gateway;
        _options = options.Value;
    }

    public async Task<SpbuSuggestionResponse> SuggestAsync(
        string keyword,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var alias =
            SearchIndexNames.SpbuAlias(_options.IndexPrefix);

        var fields = new JsonArray();

        foreach (var f in FieldKetik)
        {
            fields.Add(f);
        }

        var body = new JsonObject
        {
            ["size"] = limit,

            // Hanya field yang benar-benar ditampilkan yang diambil —
            // saran dipanggil pada hampir setiap ketukan tombol, sehingga
            // memperkecil muatan jauh lebih terasa di sini daripada di
            // tempat lain.
            ["_source"] = new JsonArray(
                "kodeSpbu",
                "nama",
                "kota",
                "provinsi"),

            ["query"] = new JsonObject
            {
                ["multi_match"] = new JsonObject
                {
                    ["query"] = keyword,
                    ["type"] = "bool_prefix",
                    ["fields"] = fields,

                    // Seluruh kata yang sudah diketik wajib cocok. Tanpa ini
                    // bool_prefix bersifat "salah satu saja", sehingga
                    // mengetik lebih banyak kata justru memperburuk saran:
                    // "jend sud" akan ikut memunculkan "Yos Sudarso" hanya
                    // karena potongan "sud" cocok.
                    ["operator"] = "and"
                }
            }
        };

        var respons =
            await _gateway.SendAsync(
                HttpMethod.POST,
                $"{alias}/_search",
                body.ToJsonString(),
                cancellationToken: cancellationToken);

        using var doc = JsonDocument.Parse(respons);

        var root = doc.RootElement;

        var items =
            root.GetProperty("hits")
                .GetProperty("hits")
                .EnumerateArray()
                .Select(h => h.GetProperty("_source"))
                .Select(s => new SpbuSuggestionItem
                {
                    KodeSpbu = Ambil(s, "kodeSpbu"),
                    Nama = Ambil(s, "nama"),
                    Kota = Ambil(s, "kota"),
                    Provinsi = Ambil(s, "provinsi")
                })
                .ToList();

        return new SpbuSuggestionResponse
        {
            Items = items,
            TookMs =
                root.TryGetProperty("took", out var t)
                    ? t.GetInt64()
                    : 0
        };
    }

    private static string Ambil(
        JsonElement source,
        string nama)
    {
        return source.TryGetProperty(nama, out var v)
            ? v.GetString() ?? string.Empty
            : string.Empty;
    }
}
