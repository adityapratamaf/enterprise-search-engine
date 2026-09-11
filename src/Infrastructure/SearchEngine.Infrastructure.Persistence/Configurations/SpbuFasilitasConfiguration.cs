using SearchEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SearchEngine.Infrastructure.Persistence.Configurations;

public class SpbuFasilitasConfiguration
    : IEntityTypeConfiguration<SpbuFasilitas>
{
    public void Configure(
        EntityTypeBuilder<SpbuFasilitas> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.SpbuId, x.FasilitasId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasOne(x => x.Spbu)
            .WithMany(x => x.Fasilitas)
            .HasForeignKey(x => x.SpbuId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Fasilitas)
            .WithMany(x => x.SpbuFasilitas)
            .HasForeignKey(x => x.FasilitasId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
