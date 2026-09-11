using SearchEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SearchEngine.Infrastructure.Persistence.Configurations;

public class SpbuProdukConfiguration
    : IEntityTypeConfiguration<SpbuProduk>
{
    public void Configure(
        EntityTypeBuilder<SpbuProduk> builder)
    {
        builder.HasKey(x => x.Id);

        // Satu SPBU tidak boleh punya dua baris untuk produk yang sama.
        builder.HasIndex(x => new { x.SpbuId, x.ProdukBbmId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        // Cascade dari SPBU: relasi ini tidak punya arti tanpa induknya.
        // Interceptor audit mengubah cascade delete menjadi soft delete,
        // sehingga baris anak ikut ditandai terhapus, bukan dibuang.
        builder.HasOne(x => x.Spbu)
            .WithMany(x => x.Produk)
            .HasForeignKey(x => x.SpbuId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ProdukBbm)
            .WithMany(x => x.SpbuProduk)
            .HasForeignKey(x => x.ProdukBbmId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
