using SearchEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SearchEngine.Infrastructure.Identity.Configurations;

public class ModuleConfiguration
    : IEntityTypeConfiguration<Module>
{
    public void Configure(
        EntityTypeBuilder<Module> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ModuleId)
            .HasMaxLength(100);

        builder.Property(x => x.ModuleName)
            .HasMaxLength(150);

        builder.Property(x => x.ModulePath)
            .HasMaxLength(250);

        builder.HasIndex(x => x.ModuleId)
            .IsUnique();
    }
}
