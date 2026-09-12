using System.ComponentModel.DataAnnotations;

namespace SearchEngine.Infrastructure.Ocr;

/// <summary>
/// Konfigurasi pembacaan tulisan pada gambar, diikat dari bagian "Ocr"
/// pada appsettings.
/// </summary>
public sealed class OcrOptions
{
    public const string SectionName = "Ocr";

    /// <summary>
    /// Letak berkas data bahasa (*.traineddata), relatif terhadap folder
    /// kerja aplikasi. Berkasnya tidak ikut repositori karena berukuran
    /// besar dan diunduh terpisah.
    /// </summary>
    [Required]
    public string TessDataPath { get; set; } = "tessdata";

    /// <summary>
    /// Bahasa yang dipakai, dipisah tanda plus. Bawaannya Indonesia
    /// didahulukan lalu Inggris: plang SPBU memuat keduanya — nama jalan
    /// berbahasa Indonesia berdampingan dengan istilah seperti "24 hours".
    ///
    /// Tiap bahasa menuntut berkasnya masing-masing: "ind+eng" memerlukan
    /// ind.traineddata dan eng.traineddata.
    /// </summary>
    [Required]
    public string Bahasa { get; set; } = "ind+eng";

    /// <summary>Batas ukuran berkas gambar yang diterima.</summary>
    [Range(1, 50)]
    public int MaksUkuranMb { get; set; } = 10;
}
