using SearchEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SearchEngine.Infrastructure.Persistence.Configurations;

public class WilayahConfiguration
    : IEntityTypeConfiguration<Wilayah>
{
    public void Configure(
        EntityTypeBuilder<Wilayah> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Kode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Nama)
            .IsRequired()
            .HasMaxLength(150);

        // Enum disimpan sebagai teks agar isi tabel terbaca langsung saat
        // diperiksa lewat SQL, dan agar penambahan anggota enum di kemudian
        // hari tidak menggeser arti baris yang sudah ada.
        builder.Property(x => x.Level)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(x => x.Kode)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        // Menopang penelusuran anak per tingkat, mis. memuat seluruh
        // kota/kabupaten milik satu provinsi.
        builder.HasIndex(x => new { x.ParentId, x.Level });

        builder.HasIndex(x => x.RegionalId);

        // Self-reference wajib Restrict: SQL Server menolak cascade pada
        // relasi yang menunjuk tabelnya sendiri.
        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Regional)
            .WithMany(x => x.Provinsi)
            .HasForeignKey(x => x.RegionalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
