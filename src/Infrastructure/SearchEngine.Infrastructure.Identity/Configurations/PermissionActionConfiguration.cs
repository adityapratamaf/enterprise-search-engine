using SearchEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SearchEngine.Infrastructure.Identity.Configurations;

public class PermissionActionConfiguration
    : IEntityTypeConfiguration<PermissionAction>
{
    public void Configure(
        EntityTypeBuilder<PermissionAction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Description)
            .HasMaxLength(300);

        builder.HasIndex(x => x.Code)
            .IsUnique();
    }
}
