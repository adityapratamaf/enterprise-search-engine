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

        var hasilEs =
            await UkurAsync(
                Ambil(SearchEngineKind.Elasticsearch),
                req,
                cancellationToken);

        var hasilSql =
            await UkurAsync(
                Ambil(SearchEngineKind.Sql),
                req,
                cancellationToken);

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
        var pemenang = esMenang ? "Elasticsearch" : "SQL";

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
        var contoh = Math.Clamp(req.Contoh, 0, 10);

        var permintaan = new SearchSpbuRequest
        {
            Search = req.Search,
            Engine = provider.Engine,
            PageNumber = 1,
            PageSize = Math.Max(1, contoh),
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

            Contoh =
                contoh == 0
                    ? []
                    : terakhir?.Items.Take(contoh).ToList() ?? []
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
