using SearchEngine.Domain.Common;
using SearchEngine.Domain.Enums;

namespace SearchEngine.Domain.Entities;

/// <summary>
/// Stasiun Pengisian Bahan Bakar Umum — objek utama yang dicari.
///
/// SQL Server berperan sebagai sumber kebenaran dan bahan indexing; seluruh
/// pencarian dilayani Elasticsearch dari dokumen turunan yang dibangun dari
/// entity ini beserta relasinya.
/// </summary>
public class Spbu
    : BaseAuditableEntity
{
    /// <summary>
    /// Kode SPBU, mis. "34.12708". Digit pertama menandakan regional dan
    /// digit kedua menandakan tipe kepemilikan; keduanya tetap disimpan
    /// sebagai data tersendiri agar tidak perlu di-parse saat query.
    /// </summary>
    public string KodeSpbu { get; set; } = default!;

    public string Nama { get; set; } = default!;

    /// <summary>
    /// Alamat jalan beserta detail di bawah tingkat wilayah yang tersimpan
    /// (kecamatan, kelurahan, RT/RW). Tetap teks bebas karena unik untuk
    /// tiap SPBU; tetap dapat dicari melalui full-text.
    /// </summary>
    public string Alamat { get; set; } = default!;

    public string? KodePos { get; set; }

    /// <summary>
    /// Wilayah administratif tempat SPBU berada. Provinsi dan regional
    /// diturunkan dengan menelusuri <see cref="Wilayah.Parent"/>.
    /// </summary>
    public Guid WilayahId { get; set; }

    public Wilayah Wilayah { get; set; } = default!;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public TipeKepemilikanSpbu TipeKepemilikan { get; set; }

    public StatusSpbu Status { get; set; } = StatusSpbu.Aktif;

    public int JumlahDispenser { get; set; }

    public int JumlahNozzle { get; set; }

    public DateTime? TanggalOperasi { get; set; }

    public string? NomorTelepon { get; set; }

    public ICollection<SpbuProduk> Produk { get; set; } = [];

    public ICollection<SpbuFasilitas> Fasilitas { get; set; } = [];
}
