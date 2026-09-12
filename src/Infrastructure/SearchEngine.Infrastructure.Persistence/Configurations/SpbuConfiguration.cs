using SearchEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SearchEngine.Infrastructure.Persistence.Configurations;

public class SpbuConfiguration
    : IEntityTypeConfiguration<Spbu>
{
    public void Configure(
        EntityTypeBuilder<Spbu> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.KodeSpbu)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Nama)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Alamat)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.KodePos)
            .HasMaxLength(10);

        builder.Property(x => x.NomorTelepon)
            .HasMaxLength(30);

        builder.Property(x => x.TipeKepemilikan)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(x => x.KodeSpbu)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        // Menopang pembacaan batch saat indexing ulang ke Elasticsearch,
        // yang menelusuri SPBU per wilayah.
        builder.HasIndex(x => x.WilayahId);

        builder.HasIndex(x => x.Status);

        // Satu angka desimal sudah cukup untuk penilaian bintang; presisi
        // yang lebih longgar hanya akan menyimpan angka yang tidak pernah
        // ditampilkan.
        builder.Property(x => x.Rating)
            .HasPrecision(2, 1);

        // Menopang penyaringan "rating minimal" dan pengurutan menurut
        // rating pada sisi SQL, supaya pembandingannya dengan Elasticsearch
        // tetap setara.
        builder.HasIndex(x => x.Rating)
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(x => x.Nama)
            .HasFilter("[IsDeleted] = 0");

        // Restrict: menghapus wilayah tidak boleh ikut menghapus SPBU di
        // dalamnya. Wilayah adalah data referensi, SPBU adalah data nyata.
        builder.HasOne(x => x.Wilayah)
            .WithMany(x => x.Spbu)
            .HasForeignKey(x => x.WilayahId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
