using SearchEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SearchEngine.Infrastructure.Persistence.Configurations;

public class FileAttachmentConfiguration
    : IEntityTypeConfiguration<FileAttachment>
{
    public void Configure(
        EntityTypeBuilder<FileAttachment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Module)
            .HasMaxLength(100);

        builder.Property(x => x.FileName)
            .HasMaxLength(255);

        builder.Property(x => x.OriginalFileName)
            .HasMaxLength(255);

        builder.Property(x => x.ContentType)
            .HasMaxLength(100);

        builder.Property(x => x.FilePath)
            .HasMaxLength(500);

        builder.Property(x => x.StorageProvider)
            .HasMaxLength(50);

        builder.HasIndex(x =>
            new
            {
                x.Module,
                x.RecordId
            });
    }
}
