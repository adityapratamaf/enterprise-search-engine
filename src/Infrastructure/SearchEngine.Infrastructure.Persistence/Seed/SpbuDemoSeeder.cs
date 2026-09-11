using SearchEngine.Domain.Entities;
using SearchEngine.Domain.Enums;
using SearchEngine.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Infrastructure.Persistence.Seed;

public sealed record SpbuSeedSummary(
    int Spbu,
    int Produk,
    int Fasilitas,
    int Dilewati);

/// <summary>
/// Membangkitkan data SPBU tiruan untuk keperluan demo dan pengujian
/// relevansi pencarian.
///
/// Sengaja BUKAN seeder startup. Data ini tidak pernah boleh masuk ke
/// database nyata hanya karena aplikasi dijalankan, sehingga pemanggilannya
/// dibuat eksplisit lewat argumen <c>--seed-demo</c>.
///
/// Sifatnya deterministik: <see cref="Seed"/> yang tetap membuat data yang
/// dihasilkan identik pada setiap kali dijalankan, sehingga hasil pencarian
/// dapat dibandingkan antar-percobaan.
/// </summary>
public static class SpbuDemoSeeder
{
    private const int Seed = 20260912;

    /// <summary>
    /// Sebaran acak koordinat di sekitar titik pusat kota/kabupaten, dalam
    /// derajat (~13 km). Cukup untuk membuat SPBU tersebar wajar tanpa
    /// keluar dari wilayahnya.
    /// </summary>
    private const double SebaranDerajat = 0.12;

    private const int UkuranBatch = 500;

    private static readonly string[] NamaJalan =
    [
        "Jenderal Sudirman", "Ahmad Yani", "Gatot Subroto", "Diponegoro",
        "Imam Bonjol", "Sisingamangaraja", "MT Haryono", "Gajah Mada",
        "Hayam Wuruk", "Pemuda", "Veteran", "Merdeka", "Pahlawan",
        "Kartini", "Cut Nyak Dien", "Teuku Umar", "Pangeran Antasari",
        "Wahid Hasyim", "Yos Sudarso", "Slamet Riyadi", "Urip Sumoharjo",
        "Panglima Polim", "Letjen Suprapto", "Dr. Soetomo", "Pattimura",
        "HOS Cokroaminoto", "KH Agus Salim", "Sultan Hasanuddin",
        "Ir. H. Juanda", "Raya Lintas Timur"
    ];

    // Digit kedua kode SPBU menandakan pola kepemilikan.
    private static readonly (TipeKepemilikanSpbu Tipe, int Digit, int Bobot)[]
        Kepemilikan =
    [
        (TipeKepemilikanSpbu.Coco, 1, 15),
        (TipeKepemilikanSpbu.Codo, 3, 15),
        (TipeKepemilikanSpbu.Dodo, 4, 70)
    ];

    private static readonly (string Kode, int Peluang)[] PeluangProduk =
    [
        ("PERTALITE", 97),
        ("BIOSOLAR", 88),
        ("PERTAMAX", 85),
        ("DEXLITE", 45),
        ("PERTAMAX_TURBO", 25),
        ("PERTAMINA_DEX", 20),
        ("PERTAMAX_GREEN_95", 8)
    ];

    private static readonly (string Kode, int Peluang)[] PeluangFasilitas =
    [
        ("TOILET", 95),
        ("MUSHOLLA", 85),
        ("ISI_ANGIN", 60),
        ("ATM", 40),
        ("MINIMARKET", 35),
        ("BUKA_24_JAM", 30),
        ("CUCI_MOBIL", 20),
        ("NITROGEN", 18),
        ("BENGKEL", 12),
        ("RUMAH_MAKAN", 10),
        ("CHARGING_EV", 5)
    ];

    /// <summary>
    /// Menggunakan tipe konkret, bukan antarmuka, karena proses ini perlu
    /// mematikan change tracking otomatis agar penulisan puluhan ribu baris
    /// tetap wajar waktunya.
    /// </summary>
    public static async Task<SpbuSeedSummary> SeedAsync(
        ApplicationBusinessDbContext context,
        int target = 10_000,
        CancellationToken cancellationToken = default)
    {
        if (await context.Spbus.AnyAsync(cancellationToken))
        {
            return new SpbuSeedSummary(0, 0, 0, Dilewati: 1);
        }

        var produkByKode =
            await context.ProdukBbms
                .AsNoTracking()
                .ToDictionaryAsync(
                    x => x.Kode,
                    x => x.Id,
                    cancellationToken);

        var fasilitasByKode =
            await context.Fasilitas
                .AsNoTracking()
                .ToDictionaryAsync(
                    x => x.Kode,
                    x => x.Id,
                    cancellationToken);

        // Kota/kabupaten beserta nomor regional yang diturunkan lewat
        // provinsi induknya.
        var kotaList =
            await context.Wilayahs
                .AsNoTracking()
                .Where(x => x.Level == LevelWilayah.KotaKabupaten
                    && x.Parent != null
                    && x.Parent.Regional != null)
                .Select(x => new
                {
                    x.Id,
                    x.Kode,
                    x.Nama,
                    RegionalNomor = x.Parent!.Regional!.Nomor
                })
                .ToListAsync(cancellationToken);

        if (kotaList.Count == 0
            || produkByKode.Count == 0
            || fasilitasByKode.Count == 0)
        {
            throw new InvalidOperationException(
                "Data referensi (wilayah, produk, fasilitas) belum tersedia. "
                + "Jalankan aplikasi sekali agar seeder referensi berjalan.");
        }

        var seedByKode =
            WilayahSeedData.Load()
                .ToDictionary(x => x.Kode);

        var totalPenduduk =
            kotaList.Sum(x =>
                seedByKode.TryGetValue(x.Kode, out var s)
                    ? (long)s.Penduduk
                    : 0L);

        var random = new Random(Seed);

        // Nomor urut kode SPBU berjalan per regional, sehingga kode tetap
        // unik secara nasional tanpa perlu memeriksa balik ke database.
        var urutanRegional = new Dictionary<int, int>();

        var bufferSpbu = new List<Spbu>(UkuranBatch);
        var bufferProduk = new List<SpbuProduk>(UkuranBatch * 4);
        var bufferFasilitas = new List<SpbuFasilitas>(UkuranBatch * 4);

        var totalSpbu = 0;
        var totalProduk = 0;
        var totalFasilitas = 0;

        var autoDetect =
            context.ChangeTracker.AutoDetectChangesEnabled;

        context.ChangeTracker.AutoDetectChangesEnabled = false;

        try
        {
            foreach (var kota in kotaList)
            {
                if (!seedByKode.TryGetValue(kota.Kode, out var geo))
                {
                    continue;
                }

                // Alokasi sebanding jumlah penduduk, minimal satu SPBU per
                // kota/kabupaten agar tidak ada wilayah yang kosong sama
                // sekali.
                var jatah =
                    totalPenduduk > 0
                        ? (int)Math.Round(
                            target * (double)geo.Penduduk / totalPenduduk)
                        : 1;

                jatah = Math.Max(1, jatah);

                for (var i = 0; i < jatah; i++)
                {
                    var spbu =
                        BuatSpbu(
                            random,
                            kota.Id,
                            kota.Nama,
                            kota.RegionalNomor,
                            geo,
                            urutanRegional);

                    bufferSpbu.Add(spbu);

                    foreach (var (kode, peluang) in PeluangProduk)
                    {
                        if (random.Next(100) < peluang
                            && produkByKode.TryGetValue(kode, out var produkId))
                        {
                            bufferProduk.Add(new SpbuProduk
                            {
                                Id = Guid.NewGuid(),
                                SpbuId = spbu.Id,
                                ProdukBbmId = produkId
                            });
                        }
                    }

                    foreach (var (kode, peluang) in PeluangFasilitas)
                    {
                        if (random.Next(100) < peluang
                            && fasilitasByKode.TryGetValue(kode, out var fasId))
                        {
                            bufferFasilitas.Add(new SpbuFasilitas
                            {
                                Id = Guid.NewGuid(),
                                SpbuId = spbu.Id,
                                FasilitasId = fasId
                            });
                        }
                    }

                    if (bufferSpbu.Count < UkuranBatch)
                    {
                        continue;
                    }

                    (var s, var p, var f) =
                        await SimpanBatchAsync(
                            context,
                            bufferSpbu,
                            bufferProduk,
                            bufferFasilitas,
                            cancellationToken);

                    totalSpbu += s;
                    totalProduk += p;
                    totalFasilitas += f;
                }
            }

            if (bufferSpbu.Count > 0)
            {
                (var s, var p, var f) =
                    await SimpanBatchAsync(
                        context,
                        bufferSpbu,
                        bufferProduk,
                        bufferFasilitas,
                        cancellationToken);

                totalSpbu += s;
                totalProduk += p;
                totalFasilitas += f;
            }
        }
        finally
        {
            context.ChangeTracker.AutoDetectChangesEnabled = autoDetect;
        }

        return new SpbuSeedSummary(
            totalSpbu,
            totalProduk,
            totalFasilitas,
            Dilewati: 0);
    }

    private static Spbu BuatSpbu(
        Random random,
        Guid wilayahId,
        string namaKota,
        int regionalNomor,
        WilayahSeedRecord geo,
        Dictionary<int, int> urutanRegional)
    {
        var tipe = PilihKepemilikan(random);

        var urutan =
            urutanRegional.TryGetValue(regionalNomor, out var current)
                ? current + 1
                : 10001;

        urutanRegional[regionalNomor] = urutan;

        // Digit pertama = regional, digit kedua = pola kepemilikan.
        // Konsisten dengan cara kode SPBU sebenarnya dibaca.
        var kode = $"{regionalNomor}{tipe.Digit}.{urutan:D5}";

        var jalan = NamaJalan[random.Next(NamaJalan.Length)];

        var kotaRingkas = RingkasNamaKota(namaKota);

        var nama =
            random.Next(100) < 60
                ? $"SPBU {jalan}"
                : $"SPBU {kotaRingkas} {jalan}";

        var dispenser = random.Next(2, 9);

        return new Spbu
        {
            Id = Guid.NewGuid(),
            KodeSpbu = kode,
            Nama = nama,
            Alamat =
                $"Jl. {jalan} No. {random.Next(1, 300)}, "
                + $"{geo.Ibukota ?? kotaRingkas}",
            KodePos = random.Next(10000, 99999).ToString(),
            WilayahId = wilayahId,
            Latitude =
                Math.Round(
                    geo.Lat + (random.NextDouble() - 0.5) * 2 * SebaranDerajat,
                    6),
            Longitude =
                Math.Round(
                    geo.Lng + (random.NextDouble() - 0.5) * 2 * SebaranDerajat,
                    6),
            TipeKepemilikan = tipe.Tipe,
            Status = PilihStatus(random),
            JumlahDispenser = dispenser,
            JumlahNozzle = dispenser * 2,
            TanggalOperasi =
                new DateTime(1995, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    .AddDays(random.Next(0, 11_000)),
            NomorTelepon =
                $"0{random.Next(21, 99)}-{random.Next(1000000, 9999999)}"
        };
    }

    private static (TipeKepemilikanSpbu Tipe, int Digit) PilihKepemilikan(
        Random random)
    {
        var undian = random.Next(100);

        var kumulatif = 0;

        foreach (var (tipe, digit, bobot) in Kepemilikan)
        {
            kumulatif += bobot;

            if (undian < kumulatif)
            {
                return (tipe, digit);
            }
        }

        return (TipeKepemilikanSpbu.Dodo, 4);
    }

    private static StatusSpbu PilihStatus(
        Random random)
    {
        var undian = random.Next(100);

        return undian switch
        {
            < 92 => StatusSpbu.Aktif,
            < 97 => StatusSpbu.Maintenance,
            _ => StatusSpbu.TidakAktif
        };
    }

    private static string RingkasNamaKota(
        string nama)
    {
        return nama
            .Replace("Kota Administrasi ", string.Empty)
            .Replace("Kabupaten ", string.Empty)
            .Replace("Kota ", string.Empty)
            .Trim();
    }

    private static async Task<(int Spbu, int Produk, int Fasilitas)>
        SimpanBatchAsync(
            ApplicationBusinessDbContext context,
            List<Spbu> spbu,
            List<SpbuProduk> produk,
            List<SpbuFasilitas> fasilitas,
            CancellationToken cancellationToken)
    {
        var jumlah = (spbu.Count, produk.Count, fasilitas.Count);

        await context.Spbus.AddRangeAsync(spbu, cancellationToken);
        await context.SpbuProduks.AddRangeAsync(produk, cancellationToken);
        await context.SpbuFasilitas.AddRangeAsync(fasilitas, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        // Entity yang sudah tersimpan dilepas dari change tracker supaya
        // pemakaian memori tetap datar sepanjang proses.
        context.ChangeTracker.Clear();

        spbu.Clear();
        produk.Clear();
        fasilitas.Clear();

        return jumlah;
    }
}
