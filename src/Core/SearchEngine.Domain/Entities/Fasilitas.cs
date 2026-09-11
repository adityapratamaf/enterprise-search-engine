using SearchEngine.Domain.Common;

namespace SearchEngine.Domain.Entities;

/// <summary>
/// Master fasilitas yang dapat dimiliki SPBU, mis. ATM, Musholla,
/// Minimarket, Charging EV. Dibuat sebagai master (bukan teks bebas)
/// supaya nilainya konsisten dan layak dipakai sebagai filter pencarian.
/// </summary>
public class Fasilitas
    : BaseAuditableEntity
{
    /// <summary>
    /// Kode unik fasilitas, mis. "ATM".
    /// </summary>
    public string Kode { get; set; } = default!;

    public string Nama { get; set; } = default!;

    public ICollection<SpbuFasilitas> SpbuFasilitas { get; set; } = [];
}
