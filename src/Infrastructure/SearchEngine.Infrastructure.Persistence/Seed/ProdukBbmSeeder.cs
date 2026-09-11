using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Domain.Entities;
using SearchEngine.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Infrastructure.Persistence.Seed;

/// <summary>
/// Menanam katalog produk BBM yang dijual di SPBU. Produk penerbangan dan
/// pelayaran sengaja tidak termasuk karena tidak disalurkan lewat SPBU.
/// Idempoten.
/// </summary>
public static class ProdukBbmSeeder
{
    private static readonly ProdukBbm[] Produk =
    [
        new()
        {
            Kode = "PERTALITE",
            Nama = "Pertalite",
            Jenis = JenisBbm.Gasoline,
            Ron = 90,
            IsSubsidi = true,
            Urutan = 1,
            Deskripsi =
                "Bensin RON 90 bersubsidi untuk kendaraan bermesin bensin. "
                + "Produk gasoline dengan konsumsi terbesar di Indonesia; "
                + "penyalurannya diatur kuota pemerintah."
        },
        new()
        {
            Kode = "PERTAMAX",
            Nama = "Pertamax",
            Jenis = JenisBbm.Gasoline,
            Ron = 92,
            Urutan = 2,
            Deskripsi =
                "Bensin RON 92 non-subsidi dengan aditif pembersih ruang "
                + "bakar. Direkomendasikan untuk kendaraan dengan rasio "
                + "kompresi 9:1 hingga 10:1."
        },
        new()
        {
            Kode = "PERTAMAX_GREEN_95",
            Nama = "Pertamax Green 95",
            Jenis = JenisBbm.Gasoline,
            Ron = 95,
            Urutan = 3,
            Deskripsi =
                "Bensin RON 95 hasil pencampuran Pertamax dengan etanol 5%. "
                + "Menekan emisi karbon dan mengurangi ketergantungan pada "
                + "bahan bakar fosil."
        },
        new()
        {
            Kode = "PERTAMAX_TURBO",
            Nama = "Pertamax Turbo",
            Jenis = JenisBbm.Gasoline,
            Ron = 98,
            Urutan = 4,
            Deskripsi =
                "Bensin RON 98 non-subsidi untuk mesin berkompresi tinggi "
                + "dan kendaraan performa. Pembakaran lebih sempurna dengan "
                + "residu paling rendah di kelas gasoline."
        },
        new()
        {
            Kode = "BIOSOLAR",
            Nama = "Biosolar",
            Jenis = JenisBbm.Diesel,
            CetaneNumber = 48,
            IsSubsidi = true,
            Urutan = 5,
            Deskripsi =
                "Solar bersubsidi campuran minyak nabati (B35) dengan cetane "
                + "number 48. Diperuntukkan bagi kendaraan diesel dan "
                + "angkutan umum; penyalurannya diatur kuota."
        },
        new()
        {
            Kode = "DEXLITE",
            Nama = "Dexlite",
            Jenis = JenisBbm.Diesel,
            CetaneNumber = 51,
            Urutan = 6,
            Deskripsi =
                "Solar non-subsidi cetane number 51 dengan kandungan sulfur "
                + "lebih rendah dari Biosolar. Pilihan menengah untuk "
                + "kendaraan diesel modern."
        },
        new()
        {
            Kode = "PERTAMINA_DEX",
            Nama = "Pertamina Dex",
            Jenis = JenisBbm.Diesel,
            CetaneNumber = 53,
            Urutan = 7,
            Deskripsi =
                "Solar non-subsidi cetane number 53, kualitas tertinggi di "
                + "kelas diesel. Untuk mesin diesel berteknologi common rail "
                + "yang menuntut bahan bakar bersih."
        }
    ];

    public static async Task SeedAsync(
        IApplicationBusinessDbContext context,
        CancellationToken cancellationToken = default)
    {
        var existing =
            (await context.ProdukBbms
                .Select(x => x.Kode)
                .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing =
            Produk
                .Where(x => !existing.Contains(x.Kode))
                .Select(x => new ProdukBbm
                {
                    Id = Guid.NewGuid(),
                    Kode = x.Kode,
                    Nama = x.Nama,
                    Deskripsi = x.Deskripsi,
                    Jenis = x.Jenis,
                    Ron = x.Ron,
                    CetaneNumber = x.CetaneNumber,
                    IsSubsidi = x.IsSubsidi,
                    Urutan = x.Urutan
                })
                .ToList();

        if (missing.Count == 0)
        {
            return;
        }

        await context.ProdukBbms.AddRangeAsync(
            missing,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
}
