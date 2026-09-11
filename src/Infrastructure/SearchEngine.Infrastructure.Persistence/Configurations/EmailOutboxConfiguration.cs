using SearchEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SearchEngine.Infrastructure.Persistence.Configurations;

public class EmailOutboxConfiguration
    : IEntityTypeConfiguration<EmailOutbox>
{
    public void Configure(
        EntityTypeBuilder<EmailOutbox> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Recipient)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(x => x.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Error)
            .HasMaxLength(500);

        builder.HasIndex(x => x.Status);
    }
}
