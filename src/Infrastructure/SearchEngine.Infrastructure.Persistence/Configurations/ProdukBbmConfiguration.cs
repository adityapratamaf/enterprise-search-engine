using SearchEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SearchEngine.Infrastructure.Persistence.Configurations;

public class ProdukBbmConfiguration
    : IEntityTypeConfiguration<ProdukBbm>
{
    public void Configure(
        EntityTypeBuilder<ProdukBbm> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Kode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Nama)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Deskripsi)
            .HasMaxLength(1000);

        // Enum disimpan sebagai teks agar isi tabel terbaca langsung saat
        // diperiksa lewat SQL, dan agar nilainya dapat dipakai apa adanya
        // sebagai keyword filter di index pencarian.
        builder.Property(x => x.Jenis)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(x => x.Kode)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(x => x.Jenis);
    }
}
