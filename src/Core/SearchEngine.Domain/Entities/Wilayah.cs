using SearchEngine.Domain.Common;
using SearchEngine.Domain.Enums;

namespace SearchEngine.Domain.Entities;

/// <summary>
/// Wilayah administratif Indonesia dalam satu tabel yang mereferensikan
/// dirinya sendiri: Provinsi → Kota/Kabupaten → Kecamatan → Kelurahan.
///
/// Satu tabel dipilih ketimbang empat tabel terpisah karena data ini hanya
/// dibaca dan nyaris tidak pernah berubah; memecahnya berarti melipatempatkan
/// konfigurasi tanpa manfaat query yang nyata. Pencarian berbasis wilayah
/// dilayani Elasticsearch, yang menerima rantai ini dalam bentuk sudah
/// diratakan saat indexing.
/// </summary>
public class Wilayah
    : BaseAuditableEntity
{
    /// <summary>
    /// Kode wilayah Kemendagri, mis. "31" (DKI Jakarta) atau "3171"
    /// (Jakarta Selatan). Panjang kode menandakan tingkatnya.
    /// </summary>
    public string Kode { get; set; } = default!;

    public string Nama { get; set; } = default!;

    public LevelWilayah Level { get; set; }

    public Guid? ParentId { get; set; }

    public Wilayah? Parent { get; set; }

    public ICollection<Wilayah> Children { get; set; } = [];

    /// <summary>
    /// Regional Pertamina yang menaungi wilayah ini. Hanya diisi pada
    /// tingkat <see cref="LevelWilayah.Provinsi"/>; tingkat di bawahnya
    /// mewarisinya melalui <see cref="Parent"/>.
    /// </summary>
    public Guid? RegionalId { get; set; }

    public Regional? Regional { get; set; }

    public ICollection<Spbu> Spbu { get; set; } = [];
}
