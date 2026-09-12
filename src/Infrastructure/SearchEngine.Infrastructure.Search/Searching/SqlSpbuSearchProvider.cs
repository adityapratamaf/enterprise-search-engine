using System.Diagnostics;

using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Search;
using SearchEngine.Application.Features.Search.DTOs;
using SearchEngine.Domain.Entities;
using SearchEngine.Domain.Enums;

using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Infrastructure.Search.Searching;

/// <summary>
/// Pencarian SPBU langsung ke SQL Server dengan <c>LIKE</c>.
///
/// Ini BUKAN mesin pencari aplikasi, melainkan pembanding. Keberadaannya
/// untuk memperlihatkan secara jujur apa yang didapat — dan apa yang hilang —
/// ketika pencarian dikerjakan apa adanya di basis data:
///
/// <list type="bullet">
/// <item>tidak ada toleransi salah ketik</item>
/// <item>tidak ada sinonim: "jl sudirman" tidak menemukan "Jalan Sudirman"</item>
/// <item>tidak ada peringkat relevansi, hanya urutan abjad</item>
/// <item>tidak ada penyorotan maupun hitungan facet</item>
/// <item>tidak ada penyaringan jarak</item>
/// </list>
///
/// Semuanya dilaporkan lewat <see cref="SearchCapabilities"/> dan
/// <see cref="SearchSpbuResponse.Catatan"/>, sehingga klien dapat
/// membedakan "mesin ini tidak mampu" dari "memang tidak ada hasil".
/// </summary>
public sealed class SqlSpbuSearchProvider
    : ISpbuSearchProvider
{
    private readonly IApplicationBusinessDbContext _context;

    public SqlSpbuSearchProvider(
        IApplicationBusinessDbContext context)
    {
        _context = context;
    }

    public SearchEngineKind Engine =>
        SearchEngineKind.Sql;

    public async Task<SearchSpbuResponse> SearchAsync(
        SearchSpbuRequest request,
        CancellationToken cancellationToken = default)
    {
        var jam = Stopwatch.StartNew();

        var catatan = new List<string>();

        var query =
            _context.Spbus
                .AsNoTracking()
                .AsQueryable();

        // ---- Kata kunci ----
        // LIKE '%kata%' tidak dapat memanfaatkan index apa pun, sehingga
        // SQL Server terpaksa memindai seluruh tabel. Inilah yang membuat
        // selisihnya terhadap Elasticsearch melebar seiring bertambahnya data.
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var kata = request.Search.Trim();

            query = query.Where(x =>
                x.Nama.Contains(kata)
                || x.KodeSpbu.Contains(kata)
                || x.Alamat.Contains(kata));
        }

        // ---- Penyaring wilayah ----

        if (Ada(request.Regional))
        {
            var nilai = request.Regional!;

            query = query.Where(x =>
                x.Wilayah.Parent != null
                && x.Wilayah.Parent.Regional != null
                && nilai.Contains(x.Wilayah.Parent.Regional.Kode));
        }

        if (Ada(request.Provinsi))
        {
            var nilai = request.Provinsi!;

            query = query.Where(x =>
                x.Wilayah.Parent != null
                && nilai.Contains(x.Wilayah.Parent.Nama));
        }

        if (Ada(request.Kota))
        {
            var nilai = request.Kota!;

            query = query.Where(x => nilai.Contains(x.Wilayah.Nama));
        }

        // ---- Penyaring relasi ----

        if (Ada(request.Produk))
        {
            var nilai = request.Produk!;

            query = query.Where(x =>
                x.Produk.Any(p =>
                    p.IsActive
                    && nilai.Contains(p.ProdukBbm.Kode)));
        }

        if (Ada(request.Fasilitas))
        {
            var nilai = request.Fasilitas!;

            query = query.Where(x =>
                x.Fasilitas.Any(f =>
                    nilai.Contains(f.Fasilitas.Kode)));
        }

        // ---- Penyaring enum ----
        // Diurai ke nilai enum lebih dulu; membandingkan hasil ToString()
        // di dalam ekspresi tidak dapat diterjemahkan ke SQL.

        var status = UraiEnum<StatusSpbu>(request.Status);

        if (status.Count > 0)
        {
            query = query.Where(x => status.Contains(x.Status));
        }

        var tipe = UraiEnum<TipeKepemilikanSpbu>(request.TipeKepemilikan);

        if (tipe.Count > 0)
        {
            query = query.Where(x => tipe.Contains(x.TipeKepemilikan));
        }

        // ---- Penyaring penilaian ----

        if (request.RatingMin.HasValue)
        {
            var minimum = (decimal)request.RatingMin.Value;

            query = query.Where(x =>
                x.Rating != null && x.Rating >= minimum);
        }

        if (request.UlasanMin.HasValue)
        {
            var minimum = request.UlasanMin.Value;

            query = query.Where(x => x.JumlahUlasan >= minimum);
        }

        // ---- Yang tidak dapat dilayani ----

        if (request.Lat.HasValue && request.RadiusKm.HasValue)
        {
            catatan.Add(
                "Penyaring jarak diabaikan: pencarian SQL tidak mendukung "
                + "operasi geospasial.");
        }

        if (request.IncludeFacets)
        {
            catatan.Add(
                "Hitungan facet tidak tersedia: setiap facet memerlukan "
                + "query agregasi tersendiri di SQL.");
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            catatan.Add(
                "Pencocokan bersifat harfiah — tanpa toleransi salah ketik, "
                + "sinonim, maupun peringkat relevansi.");
        }

        // ---- Hitung & ambil halaman ----

        var total =
            await query.CountAsync(cancellationToken);

        var terurut =
            Urutkan(query, request);

        var halaman =
            await terurut
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new
                {
                    x.Id,
                    x.KodeSpbu,
                    x.Nama,
                    x.Alamat,
                    x.KodePos,
                    Kota = x.Wilayah.Nama,
                    Provinsi =
                        x.Wilayah.Parent != null
                            ? x.Wilayah.Parent.Nama
                            : string.Empty,
                    Regional =
                        x.Wilayah.Parent != null
                        && x.Wilayah.Parent.Regional != null
                            ? x.Wilayah.Parent.Regional.Kode
                            : string.Empty,
                    RegionalNama =
                        x.Wilayah.Parent != null
                        && x.Wilayah.Parent.Regional != null
                            ? x.Wilayah.Parent.Regional.Nama
                            : string.Empty,
                    x.TipeKepemilikan,
                    x.Status,
                    x.JumlahDispenser,
                    x.JumlahNozzle,
                    x.TanggalOperasi,
                    x.NomorTelepon,
                    x.Rating,
                    x.JumlahUlasan,
                    x.Latitude,
                    x.Longitude
                })
                .ToListAsync(cancellationToken);

        var ids =
            halaman.Select(x => x.Id).ToList();

        var produk =
            await _context.SpbuProduks
                .AsNoTracking()
                .Where(x => ids.Contains(x.SpbuId) && x.IsActive)
                .Select(x => new
                {
                    x.SpbuId,
                    x.ProdukBbm.Kode,
                    x.ProdukBbm.Nama
                })
                .ToListAsync(cancellationToken);

        var fasilitas =
            await _context.SpbuFasilitas
                .AsNoTracking()
                .Where(x => ids.Contains(x.SpbuId))
                .Select(x => new
                {
                    x.SpbuId,
                    x.Fasilitas.Kode,
                    x.Fasilitas.Nama
                })
                .ToListAsync(cancellationToken);

        var produkPer =
            produk.GroupBy(x => x.SpbuId)
                .ToDictionary(g => g.Key, g => g.ToList());

        var fasilitasPer =
            fasilitas.GroupBy(x => x.SpbuId)
                .ToDictionary(g => g.Key, g => g.ToList());

        var items =
            halaman.Select(x =>
            {
                produkPer.TryGetValue(x.Id, out var p);
                fasilitasPer.TryGetValue(x.Id, out var f);

                return new SpbuSearchItem
                {
                    Id = x.Id,
                    KodeSpbu = x.KodeSpbu,
                    Nama = x.Nama,
                    Alamat = x.Alamat,
                    KodePos = x.KodePos,
                    Kota = x.Kota,
                    Provinsi = x.Provinsi,
                    Regional = x.Regional,
                    RegionalNama = x.RegionalNama,
                    TipeKepemilikan = x.TipeKepemilikan.ToString(),
                    Status = x.Status.ToString(),
                    JumlahDispenser = x.JumlahDispenser,
                    JumlahNozzle = x.JumlahNozzle,
                    TanggalOperasi = x.TanggalOperasi,
                    NomorTelepon = x.NomorTelepon,
                    Rating = x.Rating,
                    JumlahUlasan = x.JumlahUlasan,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    Produk = p?.Select(y => y.Kode).ToArray() ?? [],
                    ProdukNama = p?.Select(y => y.Nama).ToArray() ?? [],
                    Fasilitas = f?.Select(y => y.Kode).ToArray() ?? [],
                    FasilitasNama = f?.Select(y => y.Nama).ToArray() ?? [],

                    // Tidak ada skor maupun sorotan yang bisa diberikan.
                    Score = null,
                    Highlight = null,
                    JarakKm = null
                };
            })
            .ToList();

        jam.Stop();

        return new SearchSpbuResponse
        {
            Items = items,
            TotalCount = total,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages =
                (int)Math.Ceiling(total / (double)request.PageSize),
            TookMs = jam.ElapsedMilliseconds,
            Engine = SearchEngineKind.Sql,
            Facets = null,
            Catatan = catatan,
            Kemampuan = new SearchCapabilities
            {
                Highlight = false,
                Facet = false,
                Fuzzy = false,
                Relevansi = false,
                Sinonim = false,
                Geo = false
            }
        };
    }

    public async Task<long> HitungDokumenAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Spbus
            .AsNoTracking()
            .LongCountAsync(cancellationToken);
    }

    private static IQueryable<Spbu> Urutkan(
        IQueryable<Spbu> query,
        SearchSpbuRequest request)
    {
        var turun = request.IsDescending;

        // Tanpa skor relevansi, satu-satunya urutan yang masuk akal adalah
        // berdasarkan field. Id dipakai sebagai pemutus seri agar batas
        // antar-halaman tetap stabil.
        var terurut = request.SortBy?.Trim().ToLowerInvariant() switch
        {
            "kode" => turun
                ? query.OrderByDescending(x => x.KodeSpbu)
                : query.OrderBy(x => x.KodeSpbu),

            "nozzle" => turun
                ? query.OrderByDescending(x => x.JumlahNozzle)
                : query.OrderBy(x => x.JumlahNozzle),

            "rating" => turun
                ? query.OrderByDescending(x => x.Rating)
                : query.OrderBy(x => x.Rating),

            _ => turun
                ? query.OrderByDescending(x => x.Nama)
                : query.OrderBy(x => x.Nama)
        };

        return terurut.ThenBy(x => x.Id);
    }

    private static bool Ada(
        string[]? nilai)
    {
        return nilai is not null
            && nilai.Any(x => !string.IsNullOrWhiteSpace(x));
    }

    private static List<TEnum> UraiEnum<TEnum>(
        string[]? nilai)
        where TEnum : struct, Enum
    {
        if (!Ada(nilai))
        {
            return [];
        }

        var hasil = new List<TEnum>();

        foreach (var v in nilai!)
        {
            if (Enum.TryParse<TEnum>(v, ignoreCase: true, out var parsed))
            {
                hasil.Add(parsed);
            }
        }

        return hasil;
    }
}
