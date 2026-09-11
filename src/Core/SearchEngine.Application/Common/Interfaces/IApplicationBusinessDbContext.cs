using SearchEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Application.Common.Interfaces;

public interface IApplicationBusinessDbContext
{
    DbSet<Product> Products { get; }

    DbSet<FileAttachment> FileAttachments { get; }

    DbSet<EmailOutbox> EmailOutboxes { get; }

    // ---- Domain SPBU / BBM ----

    DbSet<Regional> Regionals { get; }

    DbSet<Wilayah> Wilayahs { get; }

    DbSet<ProdukBbm> ProdukBbms { get; }

    DbSet<Fasilitas> Fasilitas { get; }

    DbSet<Spbu> Spbus { get; }

    DbSet<SpbuProduk> SpbuProduks { get; }

    DbSet<SpbuFasilitas> SpbuFasilitas { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken);
}
