using System.Reflection;
using System.Text.Json;

namespace SearchEngine.Infrastructure.Persistence.Seed;

/// <summary>
/// Satu baris data wilayah administratif dari berkas seed.
///
/// Sumber: https://github.com/cahyadsn/wilayah (MIT), yang memirror kode
/// wilayah Kemendagri beserta titik koordinat dan jumlah penduduk.
/// Berkas hanya memuat tingkat provinsi dan kota/kabupaten; tingkat kode
/// ditentukan dari jumlah titik pada <see cref="Kode"/>.
/// </summary>
public sealed class WilayahSeedRecord
{
    public string Kode { get; set; } = default!;

    public string Nama { get; set; } = default!;

    public string? Ibukota { get; set; }

    public double Lat { get; set; }

    public double Lng { get; set; }

    public int Penduduk { get; set; }

    /// <summary>
    /// Kode induk, diturunkan dari <see cref="Kode"/>: "32.04" → "32".
    /// Provinsi tidak memiliki induk.
    /// </summary>
    public string? KodeParent =>
        Kode.Contains('.')
            ? Kode[..Kode.LastIndexOf('.')]
            : null;

    public bool IsProvinsi =>
        !Kode.Contains('.');
}

/// <summary>
/// Pembaca data wilayah dari embedded resource.
///
/// KOREKSI TERHADAP DATA SUMBER: baris "74.07" (Kabupaten Wakatobi) pada
/// sumber aslinya memuat <c>lng = 23.538901</c> — kehilangan angka 1 di
/// depan, sehingga titiknya jatuh di Samudra Atlantik. Nilai pada berkas ini
/// sudah dibetulkan menjadi <c>123.538901</c>. Periksa kembali bila berkas
/// ditarik ulang dari sumber.
/// </summary>
public static class WilayahSeedData
{
    private const string ResourceName =
        "SearchEngine.Infrastructure.Persistence.Seed.Data.wilayah.json";

    // Kotak batas wilayah Indonesia, dilonggarkan sedikit. Dipakai untuk
    // menolak koordinat yang jelas keliru agar kesalahan semacam Wakatobi
    // di atas gagal dengan nyaring, bukan diam-diam menghasilkan SPBU di
    // tengah samudra.
    private const double LatMin = -11.5;
    private const double LatMaks = 7.0;
    private const double LngMin = 94.0;
    private const double LngMaks = 142.0;

    private static IReadOnlyList<WilayahSeedRecord>? _cache;

    /// <summary>
    /// Memuat data wilayah dari embedded resource. Hasilnya disimpan agar
    /// pembacaan dan deserialisasi hanya terjadi sekali per proses.
    /// </summary>
    public static IReadOnlyList<WilayahSeedRecord> Load()
    {
        if (_cache is not null)
        {
            return _cache;
        }

        using var stream =
            Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException(
                $"Embedded resource '{ResourceName}' tidak ditemukan.");

        var records =
            JsonSerializer.Deserialize<List<WilayahSeedRecord>>(
                stream,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
            ?? throw new InvalidOperationException(
                "Data wilayah gagal dibaca.");

        var diLuarBatas =
            records
                .Where(x =>
                    x.Lat < LatMin || x.Lat > LatMaks
                    || x.Lng < LngMin || x.Lng > LngMaks)
                .Select(x => $"{x.Kode} ({x.Nama}) = {x.Lat},{x.Lng}")
                .ToList();

        if (diLuarBatas.Count > 0)
        {
            throw new InvalidOperationException(
                "Data wilayah memuat koordinat di luar wilayah Indonesia: "
                + string.Join("; ", diLuarBatas));
        }

        _cache = records;

        return _cache;
    }
}
