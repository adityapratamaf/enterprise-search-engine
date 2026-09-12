using System.Diagnostics;

using SearchEngine.Application.Common.Exceptions;
using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Common.Search;
using SearchEngine.Application.Features.Search.DTOs;

using MediatR;

namespace SearchEngine.Application.Features.Search.Queries.BenchmarkSearch;

/// <summary>
/// Menjalankan kata kunci yang sama pada kedua mesin lalu menyandingkan
/// hasilnya.
///
/// Beberapa keputusan pengukuran yang menjaga perbandingannya tetap setara:
///
/// <list type="bullet">
/// <item>
/// Waktu diukur dengan jam dinding di sisi aplikasi untuk KEDUA mesin,
/// bukan memakai angka <c>took</c> Elasticsearch — angka itu hanya waktu
/// eksekusi internal dan mengabaikan jaringan serta penguraian JSON.
/// </item>
/// <item>
/// Satu eksekusi pemanasan dibuang, karena eksekusi pertama selalu
/// menanggung kompilasi kueri dan pembukaan koneksi.
/// </item>
/// <item>
/// Yang dilaporkan adalah median, bukan rata-rata; sebaran waktu SQL sangat
/// lebar sehingga satu pengukuran melenceng akan menggeser rata-rata.
/// </item>
/// <item>
/// Facet dimatikan di kedua sisi, sebab hanya satu mesin yang mampu
/// menghasilkannya.
/// </item>
/// </list>
/// </summary>
public class BenchmarkSearchHandler
    : IRequestHandler<
        BenchmarkSearchQuery,
        Result<BenchmarkResponse>>
{
    private readonly IEnumerable<ISpbuSearchProvider> _providers;

    public BenchmarkSearchHandler(
        IEnumerable<ISpbuSearchProvider> providers)
    {
        _providers = providers;
    }

    public async Task<Result<BenchmarkResponse>> Handle(
        BenchmarkSearchQuery request,
        CancellationToken cancellationToken)
    {
        var req = request.Request;

        var es = Ambil(SearchEngineKind.Elasticsearch);

        var sql = Ambil(SearchEngineKind.Sql);

        var hasilEs =
            await UkurAsync(es, req, cancellationToken);

        var hasilSql =
            await UkurAsync(sql, req, cancellationToken);

        var totalDokumen =
            await es.HitungDokumenAsync(cancellationToken);

        // Yang menemukan lebih banyak dinyatakan unggul; bila jumlahnya
        // sama, yang lebih cepat.
        var esMenang =
            hasilEs.TotalHasil != hasilSql.TotalHasil
                ? hasilEs.TotalHasil > hasilSql.TotalHasil
                : hasilEs.WaktuMs <= hasilSql.WaktuMs;

        var lebihCepat = hasilEs.WaktuMs;
        var lebihLambat = hasilSql.WaktuMs;

        if (!esMenang)
        {
            (lebihCepat, lebihLambat) = (lebihLambat, lebihCepat);
        }

        var kali =
            lebihCepat > 0
                ? Math.Round(lebihLambat / lebihCepat, 1)
                : 0;

        var response = new BenchmarkResponse
        {
            Kueri = req.Search,
            Iterasi = Math.Clamp(req.Iterasi, 1, 10),
            TotalDokumen = totalDokumen,
            Pemenang =
                esMenang
                    ? SearchEngineKind.Elasticsearch
                    : SearchEngineKind.Sql,
            KaliLebihCepat = kali,
            Elasticsearch = hasilEs,
            Sql = hasilSql
        };

        return Result<BenchmarkResponse>
            .SuccessResult(
                response,
                Ringkas(hasilEs, hasilSql, esMenang, kali));
    }

    private ISpbuSearchProvider Ambil(
        SearchEngineKind engine)
    {
        return _providers.FirstOrDefault(x => x.Engine == engine)
            ?? throw new NotFoundException(
                $"Mesin pencari '{engine}' tidak tersedia.");
    }

    private static string Ringkas(
        BenchmarkEngineResult es,
        BenchmarkEngineResult sql,
        bool esMenang,
        double kali)
    {
        var pemenang = esMenang ? "Elasticsearch" : "SQL Server";

        var selisih =
            Math.Abs(es.TotalHasil - sql.TotalHasil);

        if (selisih > 0)
        {
            return $"{pemenang} unggul: {selisih:N0} hasil lebih banyak "
                + $"dan {kali}x lebih cepat.";
        }

        return $"Jumlah hasil sama ({es.TotalHasil:N0}); "
            + $"{pemenang} {kali}x lebih cepat.";
    }

    private static async Task<BenchmarkEngineResult> UkurAsync(
        ISpbuSearchProvider provider,
        BenchmarkRequest req,
        CancellationToken cancellationToken)
    {
        var permintaan = new SearchSpbuRequest
        {
            Search = req.Search,
            Engine = provider.Engine,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            SortBy = req.SortBy,
            IsDescending = req.IsDescending,

            Regional = req.Regional,
            Provinsi = req.Provinsi,
            Kota = req.Kota,
            Produk = req.Produk,
            Fasilitas = req.Fasilitas,
            Status = req.Status,
            TipeKepemilikan = req.TipeKepemilikan,

            RatingMin = req.RatingMin,
            UlasanMin = req.UlasanMin,

            Lat = req.Lat,
            Lon = req.Lon,
            RadiusKm = req.RadiusKm,

            LatMin = req.LatMin,
            LonMin = req.LonMin,
            LatMax = req.LatMax,
            LonMax = req.LonMax,

            // Facet dimatikan: hanya salah satu mesin yang mampu
            // menghasilkannya, sehingga menyertakannya akan membebani satu
            // pihak dengan pekerjaan yang tidak dilakukan pihak lain.
            IncludeFacets = false
        };

        if (req.Warmup)
        {
            await provider.SearchAsync(permintaan, cancellationToken);
        }

        var iterasi = Math.Clamp(req.Iterasi, 1, 10);

        var pengukuran = new List<double>(iterasi);

        SearchSpbuResponse? terakhir = null;

        for (var i = 0; i < iterasi; i++)
        {
            var jam = Stopwatch.StartNew();

            terakhir =
                await provider.SearchAsync(
                    permintaan,
                    cancellationToken);

            jam.Stop();

            pengukuran.Add(jam.Elapsed.TotalMilliseconds);
        }

        return new BenchmarkEngineResult
        {
            Engine = provider.Engine,
            WaktuMs = Median(pengukuran),
            TotalHasil = terakhir?.TotalCount ?? 0,
            PageNumber = terakhir?.PageNumber ?? req.PageNumber,
            PageSize = terakhir?.PageSize ?? req.PageSize,
            TotalPages = terakhir?.TotalPages ?? 0,
            Urutan = terakhir?.Urutan ?? string.Empty,
            Items = terakhir?.Items ?? []
        };
    }

    private static double Median(
        List<double> nilai)
    {
        var urut = nilai.OrderBy(x => x).ToList();

        var tengah = urut.Count / 2;

        return Math.Round(
            urut.Count % 2 == 1
                ? urut[tengah]
                : (urut[tengah - 1] + urut[tengah]) / 2,
            2);
    }
}
