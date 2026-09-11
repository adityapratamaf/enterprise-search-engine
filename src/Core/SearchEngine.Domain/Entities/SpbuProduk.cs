using SearchEngine.Domain.Common;

namespace SearchEngine.Domain.Entities;

/// <summary>
/// Produk BBM yang dijual sebuah SPBU. Satu baris = satu produk pada satu
/// SPBU.
/// </summary>
public class SpbuProduk
    : BaseAuditableEntity
{
    public Guid SpbuId { get; set; }

    public Spbu Spbu { get; set; } = default!;

    public Guid ProdukBbmId { get; set; }

    public ProdukBbm ProdukBbm { get; set; } = default!;

    /// <summary>
    /// Menandai produk yang untuk sementara tidak dijual di SPBU ini,
    /// tanpa menghapus riwayat relasinya.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
