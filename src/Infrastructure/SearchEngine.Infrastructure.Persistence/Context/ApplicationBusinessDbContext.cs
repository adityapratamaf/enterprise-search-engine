using System.Linq.Expressions;
using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Domain.Common;
using SearchEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Infrastructure.Persistence.Context;

public class ApplicationBusinessDbContext
    : DbContext,
      IApplicationBusinessDbContext
{
    public ApplicationBusinessDbContext(
        DbContextOptions<ApplicationBusinessDbContext> options)
        : base(options)
    {
    }

    public DbSet<FileAttachment> FileAttachments
        => Set<FileAttachment>();

    public DbSet<Product> Products
        => Set<Product>();

    public DbSet<EmailOutbox> EmailOutboxes
        => Set<EmailOutbox>();

    protected override void OnModelCreating(
        ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationBusinessDbContext).Assembly);

        ConfigureGlobalQueryFilters(builder);
    }

    /// <summary>
    /// Mengonfigurasi Global Query Filter untuk seluruh entity
    /// yang mengimplementasikan <see cref="IAuditableEntity"/>.
    ///
    /// Saat ini digunakan untuk menerapkan Soft Delete
    /// dengan mengecualikan data yang memiliki
    /// <c>IsDeleted = true</c>.
    /// </summary>
    /// <param name="builder">
    /// Instance <see cref="ModelBuilder"/>.
    /// </param>
    private static void ConfigureGlobalQueryFilters(
        ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (!typeof(IAuditableEntity)
                .IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var parameter =
                Expression.Parameter(
                    entityType.ClrType,
                    "e");

            var body =
                Expression.Equal(
                    Expression.Property(
                        parameter,
                        nameof(IAuditableEntity.IsDeleted)),
                    Expression.Constant(false));

            var lambda =
                Expression.Lambda(
                    body,
                    parameter);

            builder.Entity(entityType.ClrType)
                .HasQueryFilter(lambda);
        }
    }

}
