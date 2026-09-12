namespace SearchEngine.Application.Features.Search.DTOs;

/// <summary>
/// Hasil pembacaan tulisan pada sebuah gambar.
/// </summary>
public sealed class OcrResult
{
    /// <summary>
    /// Seluruh teks yang terbaca, apa adanya termasuk baris barunya.
    /// Dikembalikan utuh supaya pengguna dapat menilai sendiri apakah
    /// gambarnya terbaca dengan benar.
    /// </summary>
    public string Teks { get; set; } = string.Empty;

    /// <summary>
    /// Keyakinan rata-rata mesin OCR, 0 sampai 1. Nilai rendah biasanya
    /// menandakan foto buram, miring, atau kurang cahaya.
    /// </summary>
    public double Keyakinan { get; set; }

    /// <summary>Bahasa yang dipakai saat membaca, mis. "ind+eng".</summary>
    public string Bahasa { get; set; } = string.Empty;

    public long DurasiMs { get; set; }
}

/// <summary>
/// Hasil pencarian yang berangkat dari sebuah gambar.
///
/// Menyertakan teks mentah maupun kata kunci yang diturunkan darinya, bukan
/// hanya hasil akhirnya. Pembacaan gambar tidak pernah sempurna, sehingga
/// pengguna perlu melihat apa yang sebenarnya terbaca — dan dapat
/// memperbaiki kata kuncinya lalu mencari ulang lewat endpoint pencarian
/// biasa.
/// </summary>
public sealed class SearchByImageResponse
{
    public OcrResult Ocr { get; set; } = new();

    /// <summary>
    /// Kata kunci yang diturunkan dari teks hasil pembacaan, dan yang
    /// benar-benar dikirim ke mesin pencari.
    /// </summary>
    public string KataKunci { get; set; } = string.Empty;

    /// <summary>
    /// Kode SPBU yang terbaca pada gambar, bila ada. Ketika ini terisi,
    /// kode tersebut dipakai sebagai kata kunci karena jauh lebih menentukan
    /// daripada potongan nama atau alamat.
    /// </summary>
    public string? KodeSpbuTerdeteksi { get; set; }

    /// <summary>
    /// Hasil pencarian, bentuknya sama persis dengan yang dikembalikan
    /// endpoint pencarian biasa — memang jalur yang sama yang dipakai.
    /// </summary>
    public SearchSpbuResponse Hasil { get; set; } = new();
}
