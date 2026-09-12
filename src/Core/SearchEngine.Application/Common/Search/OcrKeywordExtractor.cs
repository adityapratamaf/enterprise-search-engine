using System.Text.RegularExpressions;

namespace SearchEngine.Application.Common.Search;

/// <summary>
/// Menurunkan kata kunci pencarian dari teks hasil pembacaan gambar.
///
/// Langkah ini menentukan kualitas hasil jauh lebih besar daripada yang
/// terlihat. Sebuah plang SPBU terbaca kira-kira begini:
///
/// <code>
/// PERTAMINA
/// SPBU 34.12708
/// Jl. Jenderal Sudirman No. 45
/// Pertamax  Pertalite  Dexlite
/// </code>
///
/// Mengirimkan seluruhnya sebagai kata kunci justru menghasilkan nol:
/// pencarian mensyaratkan sebagian besar kata cocok, sedangkan tidak ada
/// satu SPBU pun yang namanya memuat semua kata itu sekaligus.
///
/// Karena itu penyaringannya bertingkat:
/// <list type="number">
/// <item>
/// Bila ada kode SPBU, kode itulah yang dipakai — satu-satunya penanda
/// yang benar-benar unik pada sebuah plang.
/// </item>
/// <item>
/// Bila tidak, ambil kata-kata yang membedakan saja: buang kata yang
/// muncul di semua plang ("PERTAMINA", "SPBU"), buang potongan terlalu
/// pendek yang biasanya salah baca, lalu batasi jumlahnya.
/// </item>
/// </list>
/// </summary>
public static partial class OcrKeywordExtractor
{
    /// <summary>
    /// Batas jumlah kata. Semakin banyak kata, semakin sedikit SPBU yang
    /// dapat memenuhi semuanya — dan hasil pembacaan gambar terlalu rawan
    /// salah untuk dipercaya sepenuhnya.
    /// </summary>
    private const int MaksKata = 6;

    private const int PanjangKataMinimum = 3;

    /// <summary>
    /// Kata yang muncul pada hampir setiap plang sehingga tidak membedakan
    /// apa pun, atau satuan yang tidak ada gunanya dicari.
    /// </summary>
    private static readonly HashSet<string> TidakMembedakan =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "pertamina", "spbu", "pom", "bensin", "stasiun",
            "pengisian", "bahan", "bakar", "umum",
            "pasti", "pas", "prima", "way",
            "liter", "rp", "harga", "idr"
        };

    [GeneratedRegex(@"\b(\d{2})[.\-\s]?(\d{5})\b")]
    private static partial Regex PolaKodeSpbu();

    [GeneratedRegex(@"[^\p{L}\p{N}]+")]
    private static partial Regex PemisahKata();

    /// <param name="teks">Teks mentah hasil pembacaan gambar.</param>
    /// <returns>
    /// Kata kunci yang siap dicari, beserta kode SPBU bila terdeteksi.
    /// Keduanya dapat kosong bila gambar tidak memuat tulisan yang berguna.
    /// </returns>
    public static (string KataKunci, string? KodeSpbu) Ekstrak(
        string? teks)
    {
        if (string.IsNullOrWhiteSpace(teks))
        {
            return (string.Empty, null);
        }

        // Kode SPBU didahulukan. Nomornya unik secara nasional, sehingga
        // satu kode mengalahkan berapa pun banyaknya kata lain.
        var kode = PolaKodeSpbu().Match(teks);

        if (kode.Success)
        {
            var rapi =
                $"{kode.Groups[1].Value}.{kode.Groups[2].Value}";

            return (rapi, rapi);
        }

        var kata =
            PemisahKata()
                .Split(teks)
                .Where(Membedakan)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(MaksKata)
                .ToList();

        return (string.Join(' ', kata), null);
    }

    /// <summary>
    /// Menilai apakah sepotong kata layak dipakai sebagai kata kunci.
    ///
    /// Yang disingkirkan: potongan terlalu pendek — biasanya sisa salah
    /// baca; kata yang muncul pada setiap plang sehingga tidak membedakan
    /// apa pun; serta angka lepas, yang pada plang umumnya nomor rumah atau
    /// harga dan hanya menghasilkan kebisingan bila dicari.
    /// </summary>
    private static bool Membedakan(
        string kata)
    {
        return kata.Length >= PanjangKataMinimum
            && !TidakMembedakan.Contains(kata)
            && !kata.All(char.IsDigit);
    }
}
