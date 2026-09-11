using SearchEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Application.Common.Interfaces;

public interface IApplicationBusinessDbContext
{
    DbSet<Product> Products { get; }

    DbSet<FileAttachment> FileAttachments { get; }

    DbSet<EmailOutbox> EmailOutboxes { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken);
}
