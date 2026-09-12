using System.Text.Json;
using System.Text.Json.Nodes;

using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Search;
using SearchEngine.Application.Features.Search.DTOs;
using SearchEngine.Infrastructure.Search.Indexing;
using SearchEngine.Infrastructure.Search.Internal;

using Microsoft.Extensions.Options;

using HttpMethod = Elastic.Transport.HttpMethod;

namespace SearchEngine.Infrastructure.Search.Searching;

/// <summary>
/// Pencarian SPBU melalui Elasticsearch — mesin utama aplikasi ini.
///
/// Bobot field dan <c>minimum_should_match</c> di bawah bukan angka
/// karangan: keduanya ditetapkan setelah diuji langsung terhadap data nyata.
/// Tanpa <c>minimum_should_match</c>, kueri "jl sudirman" mengembalikan
/// hampir seluruh index, karena sinonim memperluas "jl" menjadi "jalan" yang
/// cocok dengan hampir setiap alamat di Indonesia.
/// </summary>
public sealed class ElasticsearchSpbuSearchProvider
    : ISpbuSearchProvider
{
    private static readonly string[] BobotField =
    [
        "kodeSpbu.digit^5",
        "nama^3",
        "alamat^2",
        "cari_semua"
    ];

    /// <summary>
    /// Kueri dua kata wajib cocok seluruhnya; lebih dari itu cukup 75%.
    /// </summary>
    private const string MinimumShouldMatch = "2<75%";

    /// <summary>
    /// Facet dengan himpunan nilai kecil dan tetap — selalu dikembalikan
    /// lengkap, sehingga ukurannya tidak perlu diatur pemanggil.
    /// </summary>
    private static readonly (string Nama, string Field, int Ukuran)[] Facets =
    [
        ("regional", "regional", 10),
        ("produk", "produk", 10),
        ("fasilitas", "fasilitas", 15),
        ("status", "status", 5),
        ("tipeKepemilikan", "tipeKepemilikan", 5)
    ];

    /// <summary>
    /// Facet wilayah: nilainya banyak (38 provinsi, 514 kota/kabupaten)
    /// sehingga ukurannya ditentukan pemanggil lewat FacetSize.
    /// </summary>
    private static readonly (string Nama, string Field)[] FacetWilayah =
    [
        ("provinsi", "provinsi"),
        ("kota", "kota")
    ];

    private readonly ElasticsearchGateway _gateway;

    private readonly ElasticsearchOptions _options;

    public ElasticsearchSpbuSearchProvider(
        ElasticsearchGateway gateway,
        IOptions<ElasticsearchOptions> options)
    {
        _gateway = gateway;
        _options = options.Value;
    }

    public SearchEngineKind Engine =>
        SearchEngineKind.Elasticsearch;

    public async Task<SearchSpbuResponse> SearchAsync(
        SearchSpbuRequest request,
        CancellationToken cancellationToken = default)
    {
        var alias =
            SearchIndexNames.SpbuAlias(_options.IndexPrefix);

        var body =
            SusunKueri(request);

        var respons =
            await _gateway.SendAsync(
                HttpMethod.POST,
                $"{alias}/_search",
                body.ToJsonString(),
                cancellationToken: cancellationToken);

        return Baca(respons, request);
    }

    public async Task<long> HitungDokumenAsync(
        CancellationToken cancellationToken = default)
    {
        var alias =
            SearchIndexNames.SpbuAlias(_options.IndexPrefix);

        var respons =
            await _gateway.SendAsync(
                HttpMethod.GET,
                $"{alias}/_count",
                null,
                cancellationToken: cancellationToken);

        using var doc = JsonDocument.Parse(respons);

        return doc.RootElement.TryGetProperty("count", out var c)
            ? c.GetInt64()
            : 0;
    }

    // =====================================================================
    // MENYUSUN KUERI
    // =====================================================================

    private JsonObject SusunKueri(
        SearchSpbuRequest request)
    {
        var adaKeyword =
            !string.IsNullOrWhiteSpace(request.Search);

        var adaGeo =
            request.Lat.HasValue
            && request.Lon.HasValue
            && request.RadiusKm.HasValue;

        var root = new JsonObject
        {
            ["from"] = (request.PageNumber - 1) * request.PageSize,
            ["size"] = request.PageSize,

            // Tanpa ini Elasticsearch berhenti menghitung di 10.000 dan
            // melaporkan angka itu sebagai total — membuat "sekian hasil"
            // salah untuk kueri yang luas.
            ["track_total_hits"] = true
        };

        var boolQuery = new JsonObject();

        if (adaKeyword)
        {
            var fields = new JsonArray();

            foreach (var f in BobotField)
            {
                fields.Add(f);
            }

            var should = new JsonArray
            {
                // Klausa utama: kata-kata utuh, toleran salah ketik.
                new JsonObject
                {
                    ["multi_match"] = new JsonObject
                    {
                        ["query"] = request.Search,
                        ["fields"] = fields,
                        ["fuzziness"] = "AUTO",
                        ["minimum_should_match"] = MinimumShouldMatch
                    }
                },

                // Klausa pelengkap: kata terakhir diperlakukan sebagai
                // awalan. Tanpa ini, menekan Enter di tengah kata —
                // "sudir", "jend sud" — tidak mengembalikan apa pun, karena
                // pencocokan kata utuh menganggapnya kata lain dan jarak
                // ketiknya terlalu jauh untuk ditolong fuzzy.
                new JsonObject
                {
                    ["multi_match"] = new JsonObject
                    {
                        ["query"] = request.Search,
                        ["type"] = "phrase_prefix",
                        ["fields"] = new JsonArray("nama^3", "alamat^2"),
                        ["max_expansions"] = 50
                    }
                }
            };

            boolQuery["should"] = should;
            boolQuery["minimum_should_match"] = 1;
        }
        else
        {
            boolQuery["must"] = new JsonArray
            {
                new JsonObject
                {
                    ["match_all"] = new JsonObject()
                }
            };
        }

        var filter = new JsonArray();

        TambahTerms(filter, "regional", request.Regional);
        TambahTerms(filter, "provinsi", request.Provinsi);
        TambahTerms(filter, "kota", request.Kota);
        TambahTerms(filter, "produk", request.Produk);
        TambahTerms(filter, "fasilitas", request.Fasilitas);
        TambahTerms(filter, "status", request.Status);
        TambahTerms(filter, "tipeKepemilikan", request.TipeKepemilikan);

        // Rating kosong otomatis tersaring keluar: klausa range tidak
        // pernah cocok dengan field yang tidak ada nilainya.
        TambahMinimum(filter, "rating", request.RatingMin);
        TambahMinimum(filter, "jumlahUlasan", request.UlasanMin);

        // Kotak peta: mengikuti bentuk layar, bukan lingkaran seperti
        // radius. Keduanya boleh dipakai bersamaan dan saling mempersempit.
        if (request.LatMin.HasValue && request.LonMin.HasValue
            && request.LatMax.HasValue && request.LonMax.HasValue)
        {
            filter.Add(new JsonObject
            {
                ["geo_bounding_box"] = new JsonObject
                {
                    ["lokasi"] = new JsonObject
                    {
                        ["top_left"] = new JsonObject
                        {
                            ["lat"] = request.LatMax,
                            ["lon"] = request.LonMin
                        },
                        ["bottom_right"] = new JsonObject
                        {
                            ["lat"] = request.LatMin,
                            ["lon"] = request.LonMax
                        }
                    }
                }
            });
        }

        if (adaGeo)
        {
            filter.Add(new JsonObject
            {
                ["geo_distance"] = new JsonObject
                {
                    ["distance"] = $"{request.RadiusKm}km",
                    ["lokasi"] = new JsonObject
                    {
                        ["lat"] = request.Lat,
                        ["lon"] = request.Lon
                    }
                }
            });
        }

        boolQuery["filter"] = filter;

        root["query"] = new JsonObject
        {
            ["bool"] = boolQuery
        };

        // Penyorotan hanya bermakna bila ada kata yang dicari.
        // require_field_match dimatikan supaya kecocokan yang terjadi pada
        // field gabungan cari_semua tetap menyorot nama dan alamat aslinya.
        if (adaKeyword)
        {
            root["highlight"] = new JsonObject
            {
                ["pre_tags"] = new JsonArray("<mark>"),
                ["post_tags"] = new JsonArray("</mark>"),
                ["require_field_match"] = false,
                ["fields"] = new JsonObject
                {
                    ["nama"] = new JsonObject
                    {
                        ["number_of_fragments"] = 0
                    },
                    ["alamat"] = new JsonObject
                    {
                        ["number_of_fragments"] = 0
                    }
                }
            };
        }

        if (request.IncludeFacets)
        {
            var aggs = new JsonObject();

            foreach (var (nama, field, ukuran) in Facets)
            {
                aggs[nama] = new JsonObject
                {
                    ["terms"] = new JsonObject
                    {
                        ["field"] = field,
                        ["size"] = ukuran
                    }
                };
            }

            var ukuranWilayah =
                Math.Clamp(request.FacetSize, 1, 600);

            foreach (var (nama, field) in FacetWilayah)
            {
                aggs[nama] = new JsonObject
                {
                    ["terms"] = new JsonObject
                    {
                        ["field"] = field,
                        ["size"] = ukuranWilayah
                    }
                };
            }

            root["aggs"] = aggs;
        }

        root["sort"] =
            SusunUrutan(request, adaKeyword, adaGeo);

        return root;
    }

    private static JsonArray SusunUrutan(
        SearchSpbuRequest request,
        bool adaKeyword,
        bool adaGeo)
    {
        var arah =
            request.IsDescending ? "desc" : "asc";

        var sort = new JsonArray();

        switch (request.SortBy?.Trim().ToLowerInvariant())
        {
            case "nama":
                sort.Add(new JsonObject { ["nama.keyword"] = arah });
                break;

            case "kode":
                sort.Add(new JsonObject { ["kodeSpbu"] = arah });
                break;

            case "nozzle":
                sort.Add(new JsonObject { ["jumlahNozzle"] = arah });
                break;

            case "rating":
                // Rating kosong ditempatkan paling belakang, bukan
                // dianggap bernilai nol.
                sort.Add(new JsonObject
                {
                    ["rating"] = new JsonObject
                    {
                        ["order"] = arah,
                        ["missing"] = "_last"
                    }
                });
                break;

            case "jarak" when adaGeo:
                sort.Add(new JsonObject
                {
                    ["_geo_distance"] = new JsonObject
                    {
                        ["lokasi"] = new JsonObject
                        {
                            ["lat"] = request.Lat,
                            ["lon"] = request.Lon
                        },
                        ["order"] = arah,
                        ["unit"] = "km"
                    }
                });
                break;

            default:
                // Bawaan: relevansi bila ada kata kunci, abjad bila tidak —
                // mengurutkan hasil match_all berdasarkan skor tidak ada
                // artinya karena seluruh dokumen berskor sama.
                if (adaKeyword)
                {
                    sort.Add("_score");
                }
                else
                {
                    sort.Add(new JsonObject { ["nama.keyword"] = "asc" });
                }

                break;
        }

        // Pemutus seri supaya batas antar-halaman tidak pernah bergeser
        // ketika banyak dokumen memiliki nilai urut yang sama.
        sort.Add(new JsonObject { ["id"] = "asc" });

        return sort;
    }

    private static void TambahMinimum(
        JsonArray filter,
        string field,
        double? minimum)
    {
        if (minimum is null)
        {
            return;
        }

        filter.Add(new JsonObject
        {
            ["range"] = new JsonObject
            {
                [field] = new JsonObject
                {
                    ["gte"] = minimum
                }
            }
        });
    }

    private static void TambahTerms(
        JsonArray filter,
        string field,
        string[]? nilai)
    {
        if (nilai is null || nilai.Length == 0)
        {
            return;
        }

        var arr = new JsonArray();

        foreach (var v in nilai.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            arr.Add(v);
        }

        if (arr.Count == 0)
        {
            return;
        }

        filter.Add(new JsonObject
        {
            ["terms"] = new JsonObject
            {
                [field] = arr
            }
        });
    }

    // =====================================================================
    // MEMBACA TANGGAPAN
    // =====================================================================

    private static SearchSpbuResponse Baca(
        string respons,
        SearchSpbuRequest request)
    {
        using var doc = JsonDocument.Parse(respons);

        var root = doc.RootElement;

        var took =
            root.TryGetProperty("took", out var t)
                ? t.GetInt64()
                : 0;

        var hits = root.GetProperty("hits");

        var total =
            hits.GetProperty("total").GetProperty("value").GetInt32();

        var adaGeo =
            request.Lat.HasValue && request.Lon.HasValue;

        var items = new List<SpbuSearchItem>();

        foreach (var hit in hits.GetProperty("hits").EnumerateArray())
        {
            var dokumen =
                JsonSerializer.Deserialize<SpbuDocument>(
                    hit.GetProperty("_source").GetRawText());

            if (dokumen is null)
            {
                continue;
            }

            var item = new SpbuSearchItem
            {
                Id = dokumen.Id,
                KodeSpbu = dokumen.KodeSpbu,
                Nama = dokumen.Nama,
                Alamat = dokumen.Alamat,
                KodePos = dokumen.KodePos,
                Kota = dokumen.Kota,
                Provinsi = dokumen.Provinsi,
                Regional = dokumen.Regional,
                RegionalNama = dokumen.RegionalNama,
                TipeKepemilikan = dokumen.TipeKepemilikan,
                Status = dokumen.Status,
                JumlahDispenser = dokumen.JumlahDispenser,
                JumlahNozzle = dokumen.JumlahNozzle,
                TanggalOperasi = dokumen.TanggalOperasi,
                NomorTelepon = dokumen.NomorTelepon,
                Rating = dokumen.Rating,
                JumlahUlasan = dokumen.JumlahUlasan,
                Latitude = dokumen.Lokasi.Lat,
                Longitude = dokumen.Lokasi.Lon,
                Produk = dokumen.Produk,
                ProdukNama = dokumen.ProdukNama,
                Fasilitas = dokumen.Fasilitas,
                FasilitasNama = dokumen.FasilitasNama,

                Score =
                    hit.TryGetProperty("_score", out var s)
                    && s.ValueKind == JsonValueKind.Number
                        ? s.GetDouble()
                        : null,

                Highlight =
                    hit.TryGetProperty("highlight", out var h)
                        ? h.EnumerateObject()
                            .ToDictionary(
                                p => p.Name,
                                p => p.Value
                                    .EnumerateArray()
                                    .Select(x => x.GetString() ?? string.Empty)
                                    .ToArray())
                        : null,

                JarakKm =
                    adaGeo
                        ? Haversine(
                            request.Lat!.Value,
                            request.Lon!.Value,
                            dokumen.Lokasi.Lat,
                            dokumen.Lokasi.Lon)
                        : null
            };

            items.Add(item);
        }

        Dictionary<string, List<FacetBucket>>? facets = null;

        if (root.TryGetProperty("aggregations", out var aggs))
        {
            facets = [];

            foreach (var agg in aggs.EnumerateObject())
            {
                if (!agg.Value.TryGetProperty("buckets", out var buckets))
                {
                    continue;
                }

                facets[agg.Name] =
                    buckets.EnumerateArray()
                        .Select(b => new FacetBucket
                        {
                            Nilai =
                                b.GetProperty("key").ToString(),
                            Jumlah =
                                b.GetProperty("doc_count").GetInt64()
                        })
                        .ToList();
            }
        }

        return new SearchSpbuResponse
        {
            Items = items,
            TotalCount = total,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages =
                (int)Math.Ceiling(total / (double)request.PageSize),
            TookMs = took,
            Engine = SearchEngineKind.Elasticsearch,
            Urutan =
                LabelUrutan.Susun(
                    SearchEngineKind.Elasticsearch,
                    request.SortBy,
                    request.IsDescending,
                    !string.IsNullOrWhiteSpace(request.Search),
                    request.Lat.HasValue),
            Facets = facets,
            Kemampuan = new SearchCapabilities
            {
                Highlight = true,
                Facet = request.IncludeFacets,
                Fuzzy = true,
                Relevansi = true,
                Sinonim = true,
                Geo = true
            }
        };
    }

    /// <summary>
    /// Jarak lingkaran besar antara dua titik, dalam kilometer. Dihitung di
    /// sisi aplikasi supaya jaraknya tetap tersedia meskipun hasil tidak
    /// diurutkan berdasarkan jarak.
    /// </summary>
    private static double Haversine(
        double lat1,
        double lon1,
        double lat2,
        double lon2)
    {
        const double JariJariBumiKm = 6371.0;

        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;

        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
            + Math.Cos(lat1 * Math.PI / 180)
            * Math.Cos(lat2 * Math.PI / 180)
            * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        return Math.Round(
            JariJariBumiKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)),
            2);
    }
}
