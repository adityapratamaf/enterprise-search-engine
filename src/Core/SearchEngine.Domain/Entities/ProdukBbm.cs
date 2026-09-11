using SearchEngine.Domain.Common;
using SearchEngine.Domain.Enums;

namespace SearchEngine.Domain.Entities;

/// <summary>
/// Master produk BBM yang dijual di SPBU.
///
/// <code>
/// Gasoline  ─ Pertalite, Pertamax, Pertamax Turbo, Pertamax Green 95
/// Diesel    ─ Biosolar, Dexlite, Pertamina Dex
/// </code>
///
/// Katalog sengaja dibiarkan datar. Golongan bahan bakar untuk SPBU hanya
/// ada dua dan bersifat tetap, sehingga <see cref="JenisBbm"/> cukup sebagai
/// enum — disimpan sebagai teks, sehingga tetap dapat dipakai sebagai filter
/// pencarian ("SPBU yang menjual Diesel") tanpa perlu tabel hierarki.
///
/// Produk untuk penerbangan (Avtur, Avgas) dan pelayaran (MGO, MDO, MFO,
/// LSFO) tidak termasuk di sini: keduanya tidak disalurkan lewat SPBU,
/// melainkan lewat DPPU dan bunker station.
/// </summary>
public class ProdukBbm
    : BaseAuditableEntity
{
    /// <summary>
    /// Kode unik produk, mis. "PERTALITE". Dipakai sebagai nilai filter
    /// pada index pencarian.
    /// </summary>
    public string Kode { get; set; } = default!;

    public string Nama { get; set; } = default!;

    /// <summary>
    /// Penjelasan singkat produk: peruntukan, karakteristik, dan pembeda
    /// dari produk lain di golongan yang sama.
    /// </summary>
    public string? Deskripsi { get; set; }

    public JenisBbm Jenis { get; set; }

    /// <summary>
    /// Research Octane Number. Hanya relevan untuk
    /// <see cref="JenisBbm.Gasoline"/>.
    /// </summary>
    public int? Ron { get; set; }

    /// <summary>
    /// Cetane number. Hanya relevan untuk <see cref="JenisBbm.Diesel"/>.
    /// </summary>
    public int? CetaneNumber { get; set; }

    /// <summary>
    /// Menandai produk bersubsidi (Pertalite, Biosolar). Dipakai untuk
    /// memisahkan analisis konsumsi subsidi pada tahap berikutnya.
    /// </summary>
    public bool IsSubsidi { get; set; }

    /// <summary>
    /// Produk yang tidak lagi dipasarkan tetap disimpan agar data historis
    /// yang menunjuk ke sana tetap utuh.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Urutan tampil pada daftar produk.
    /// </summary>
    public int Urutan { get; set; }

    public ICollection<SpbuProduk> SpbuProduk { get; set; } = [];
}
