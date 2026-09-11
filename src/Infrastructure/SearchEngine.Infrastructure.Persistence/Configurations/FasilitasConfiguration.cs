using SearchEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SearchEngine.Infrastructure.Persistence.Configurations;

public class FasilitasConfiguration
    : IEntityTypeConfiguration<Fasilitas>
{
    public void Configure(
        EntityTypeBuilder<Fasilitas> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Kode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Nama)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.Kode)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
