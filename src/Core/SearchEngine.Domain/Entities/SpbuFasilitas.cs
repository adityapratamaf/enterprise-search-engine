using SearchEngine.Domain.Common;

namespace SearchEngine.Domain.Entities;

/// <summary>
/// Fasilitas yang tersedia di sebuah SPBU. Satu baris = satu fasilitas
/// pada satu SPBU.
/// </summary>
public class SpbuFasilitas
    : BaseAuditableEntity
{
    public Guid SpbuId { get; set; }

    public Spbu Spbu { get; set; } = default!;

    public Guid FasilitasId { get; set; }

    public Fasilitas Fasilitas { get; set; } = default!;
}
