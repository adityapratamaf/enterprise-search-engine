using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Domain.Entities;
using SearchEngine.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Infrastructure.Persistence.Seed;

/// <summary>
/// Menanam 38 provinsi dan 514 kota/kabupaten dari embedded resource.
///
/// Provinsi ditanam lebih dulu karena kota/kabupaten membutuhkan
/// <see cref="Wilayah.ParentId"/>-nya. Idempoten: hanya kode yang belum ada
/// yang ditambahkan, sehingga aman dijalankan setiap kali aplikasi start.
/// </summary>
public static class WilayahSeeder
{
    public static async Task SeedAsync(
        IApplicationBusinessDbContext context,
        CancellationToken cancellationToken = default)
    {
        var seed = WilayahSeedData.Load();

        var existingKode =
            (await context.Wilayahs
                .Select(x => x.Kode)
                .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // ---- Provinsi ----

        var regionalByNomor =
            await context.Regionals
                .AsNoTracking()
                .ToDictionaryAsync(
                    x => x.Nomor,
                    x => x.Id,
                    cancellationToken);

        var provinsiBaru =
            seed
                .Where(x => x.IsProvinsi
                    && !existingKode.Contains(x.Kode))
                .Select(x => new Wilayah
                {
                    Id = Guid.NewGuid(),
                    Kode = x.Kode,
                    Nama = x.Nama,
                    Level = LevelWilayah.Provinsi,
                    ParentId = null,
                    RegionalId =
                        RegionalSeeder.ProvinsiRegional
                            .TryGetValue(x.Kode, out var nomor)
                        && regionalByNomor.TryGetValue(nomor, out var regionalId)
                            ? regionalId
                            : null
                })
                .ToList();

        if (provinsiBaru.Count > 0)
        {
            await context.Wilayahs.AddRangeAsync(
                provinsiBaru,
                cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
        }

        // ---- Kota / Kabupaten ----

        // Dibaca ulang setelah provinsi tersimpan agar mencakup baris yang
        // baru saja ditambahkan maupun yang sudah ada sebelumnya.
        var idByKode =
            await context.Wilayahs
                .AsNoTracking()
                .ToDictionaryAsync(
                    x => x.Kode,
                    x => x.Id,
                    cancellationToken);

        var kotaBaru =
            seed
                .Where(x => !x.IsProvinsi
                    && !existingKode.Contains(x.Kode)
                    && x.KodeParent is not null
                    && idByKode.ContainsKey(x.KodeParent))
                .Select(x => new Wilayah
                {
                    Id = Guid.NewGuid(),
                    Kode = x.Kode,
                    Nama = x.Nama,
                    Level = LevelWilayah.KotaKabupaten,
                    ParentId = idByKode[x.KodeParent!],
                    RegionalId = null
                })
                .ToList();

        if (kotaBaru.Count == 0)
        {
            return;
        }

        await context.Wilayahs.AddRangeAsync(
            kotaBaru,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
}
