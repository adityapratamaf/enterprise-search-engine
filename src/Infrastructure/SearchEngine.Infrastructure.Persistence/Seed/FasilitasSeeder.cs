using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Infrastructure.Persistence.Seed;

/// <summary>
/// Menanam master fasilitas SPBU. Nilainya dipakai sebagai filter pencarian,
/// sehingga sengaja dibatasi pada daftar tertutup agar konsisten. Idempoten.
/// </summary>
public static class FasilitasSeeder
{
    private static readonly (string Kode, string Nama)[] Fasilitas =
    [
        ("TOILET",       "Toilet"),
        ("MUSHOLLA",     "Musholla"),
        ("ATM",          "ATM"),
        ("MINIMARKET",   "Minimarket"),
        ("ISI_ANGIN",    "Isi Angin"),
        ("NITROGEN",     "Pengisian Nitrogen"),
        ("CUCI_MOBIL",   "Cuci Mobil"),
        ("BENGKEL",      "Bengkel"),
        ("RUMAH_MAKAN",  "Rumah Makan"),
        ("CHARGING_EV",  "Charging Station EV"),
        ("BUKA_24_JAM",  "Buka 24 Jam")
    ];

    public static async Task SeedAsync(
        IApplicationBusinessDbContext context,
        CancellationToken cancellationToken = default)
    {
        var existing =
            (await context.Fasilitas
                .Select(x => x.Kode)
                .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing =
            Fasilitas
                .Where(x => !existing.Contains(x.Kode))
                .Select(x => new Fasilitas
                {
                    Id = Guid.NewGuid(),
                    Kode = x.Kode,
                    Nama = x.Nama
                })
                .ToList();

        if (missing.Count == 0)
        {
            return;
        }

        await context.Fasilitas.AddRangeAsync(
            missing,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
}
