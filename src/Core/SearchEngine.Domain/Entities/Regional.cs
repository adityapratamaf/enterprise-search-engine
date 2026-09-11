using SearchEngine.Domain.Common;

namespace SearchEngine.Domain.Entities;

/// <summary>
/// Regional Pertamina (8 wilayah pemasaran). Satu regional menaungi
/// beberapa provinsi, sehingga regional sebuah SPBU selalu diturunkan
/// melalui provinsinya — bukan disimpan langsung pada SPBU — agar tidak
/// mungkin terjadi SPBU beralamat di Jawa tetapi ter-tag Sulawesi.
/// </summary>
public class Regional
    : BaseAuditableEntity
{
    /// <summary>
    /// Nomor resmi regional (1-8). Nilai ini sekaligus merupakan digit
    /// pertama kode SPBU, sehingga dapat dipakai memvalidasi silang bahwa
    /// kode SPBU konsisten dengan lokasi administratifnya.
    /// </summary>
    public int Nomor { get; set; }

    /// <summary>
    /// Kode singkat regional, mis. "JBB". Dipakai sebagai nilai filter
    /// pada index pencarian.
    /// </summary>
    public string Kode { get; set; } = default!;

    /// <summary>
    /// Nama operasional, mis. "Jawa Bagian Barat".
    /// </summary>
    public string Nama { get; set; } = default!;

    /// <summary>
    /// Provinsi yang dinaungi regional ini.
    /// </summary>
    public ICollection<Wilayah> Provinsi { get; set; } = [];
}
