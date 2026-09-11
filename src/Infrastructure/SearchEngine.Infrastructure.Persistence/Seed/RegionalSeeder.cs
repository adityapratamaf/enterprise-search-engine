using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Infrastructure.Persistence.Seed;

/// <summary>
/// Menanam 8 Regional Pertamina beserta pemetaan provinsi yang dinaunginya.
/// Idempoten: hanya menambahkan regional yang belum ada.
/// </summary>
public static class RegionalSeeder
{
    private static readonly (int Nomor, string Kode, string Nama)[] Regionals =
    [
        (1, "SUMBAGUT",     "Sumatera Bagian Utara"),
        (2, "SUMBAGSEL",    "Sumatera Bagian Selatan"),
        (3, "JBB",          "Jawa Bagian Barat"),
        (4, "JBT",          "Jawa Bagian Tengah"),
        (5, "JATIMBALINUS", "Jawa Timur, Bali & Nusa Tenggara"),
        (6, "KALIMANTAN",   "Kalimantan"),
        (7, "SULAWESI",     "Sulawesi"),
        (8, "PAPUAMALUKU",  "Papua & Maluku")
    ];

    /// <summary>
    /// Pemetaan kode provinsi Kemendagri ke nomor regional Pertamina.
    /// Mencakup seluruh 38 provinsi.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, int> ProvinsiRegional =
        new Dictionary<string, int>
        {
            // 1 — Sumbagut
            ["11"] = 1, // Aceh
            ["12"] = 1, // Sumatera Utara
            ["13"] = 1, // Sumatera Barat
            ["14"] = 1, // Riau
            ["21"] = 1, // Kepulauan Riau

            // 2 — Sumbagsel
            ["15"] = 2, // Jambi
            ["16"] = 2, // Sumatera Selatan
            ["17"] = 2, // Bengkulu
            ["18"] = 2, // Lampung
            ["19"] = 2, // Kepulauan Bangka Belitung

            // 3 — JBB
            ["31"] = 3, // DKI Jakarta
            ["32"] = 3, // Jawa Barat
            ["36"] = 3, // Banten

            // 4 — JBT
            ["33"] = 4, // Jawa Tengah
            ["34"] = 4, // DI Yogyakarta

            // 5 — Jatimbalinus
            ["35"] = 5, // Jawa Timur
            ["51"] = 5, // Bali
            ["52"] = 5, // Nusa Tenggara Barat
            ["53"] = 5, // Nusa Tenggara Timur

            // 6 — Kalimantan
            ["61"] = 6, // Kalimantan Barat
            ["62"] = 6, // Kalimantan Tengah
            ["63"] = 6, // Kalimantan Selatan
            ["64"] = 6, // Kalimantan Timur
            ["65"] = 6, // Kalimantan Utara

            // 7 — Sulawesi
            ["71"] = 7, // Sulawesi Utara
            ["72"] = 7, // Sulawesi Tengah
            ["73"] = 7, // Sulawesi Selatan
            ["74"] = 7, // Sulawesi Tenggara
            ["75"] = 7, // Gorontalo
            ["76"] = 7, // Sulawesi Barat

            // 8 — Papua & Maluku
            ["81"] = 8, // Maluku
            ["82"] = 8, // Maluku Utara
            ["91"] = 8, // Papua
            ["92"] = 8, // Papua Barat
            ["93"] = 8, // Papua Selatan
            ["94"] = 8, // Papua Tengah
            ["95"] = 8, // Papua Pegunungan
            ["96"] = 8  // Papua Barat Daya
        };

    public static async Task SeedAsync(
        IApplicationBusinessDbContext context,
        CancellationToken cancellationToken = default)
    {
        var existing =
            (await context.Regionals
                .Select(x => x.Nomor)
                .ToListAsync(cancellationToken))
            .ToHashSet();

        var missing =
            Regionals
                .Where(x => !existing.Contains(x.Nomor))
                .Select(x => new Regional
                {
                    Id = Guid.NewGuid(),
                    Nomor = x.Nomor,
                    Kode = x.Kode,
                    Nama = x.Nama
                })
                .ToList();

        if (missing.Count == 0)
        {
            return;
        }

        await context.Regionals.AddRangeAsync(
            missing,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
}
