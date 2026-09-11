using SearchEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SearchEngine.Infrastructure.Persistence.Configurations;

public class ProductConfiguration
    : IEntityTypeConfiguration<Product>
{
    public void Configure(
        EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200);

        builder.Property(x => x.Code)
            .HasMaxLength(50);

        builder.Property(x => x.Price)
            .HasPrecision(18, 2);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        // Supports the default Product listing (ORDER BY Name, Id) and prefix
        // searches on Name. Filtered to [IsDeleted] = 0 to match the global
        // soft-delete query filter, keeping the index small and write-cheap.
        // The clustered PK (Id) is the implicit tie-breaker in this index, so
        // ORDER BY Name, Id is served without a separate sort.
        builder.HasIndex(x => x.Name)
            .HasFilter("[IsDeleted] = 0");
    }
}
