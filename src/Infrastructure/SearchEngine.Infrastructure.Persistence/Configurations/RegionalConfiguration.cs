using SearchEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SearchEngine.Infrastructure.Persistence.Configurations;

public class RegionalConfiguration
    : IEntityTypeConfiguration<Regional>
{
    public void Configure(
        EntityTypeBuilder<Regional> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Kode)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.Nama)
            .IsRequired()
            .HasMaxLength(100);

        // Unique index disaring ke baris hidup saja. Tanpa filter ini,
        // sebuah baris yang sudah di-soft delete tetap menahan kodenya dan
        // membuat baris baru dengan kode sama ditolak database.
        builder.HasIndex(x => x.Kode)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(x => x.Nomor)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
