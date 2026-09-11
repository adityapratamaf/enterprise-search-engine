using SearchEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SearchEngine.Infrastructure.Identity.Configurations;

public class AuditActionConfiguration
    : IEntityTypeConfiguration<AuditAction>
{
    public void Configure(
        EntityTypeBuilder<AuditAction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserEmail)
            .HasMaxLength(256);

        builder.Property(x => x.Action)
            .HasMaxLength(50);

        builder.Property(x => x.Module)
            .HasMaxLength(100);

        builder.Property(x => x.TableName)
            .HasMaxLength(150);

        builder.Property(x => x.RecordId)
            .HasMaxLength(100);

        builder.Property(x => x.IpAddress)
            .HasMaxLength(64);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(512);

        builder.HasIndex(x => x.CreatedAt);
    }
}
