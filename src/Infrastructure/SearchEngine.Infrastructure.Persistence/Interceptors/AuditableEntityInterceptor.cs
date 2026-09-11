using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SearchEngine.Infrastructure.Persistence.Interceptors;

public class AuditableEntityInterceptor
    : SaveChangesInterceptor
{
    private readonly ICurrentUserService
        _currentUserService;

    public AuditableEntityInterceptor(
        ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override InterceptionResult<int>
        SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChanges(
            eventData,
            result);
    }

    public override ValueTask<
        InterceptionResult<int>>
        SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
    {
        UpdateEntities(
            eventData.Context);

        return base.SavingChangesAsync(
            eventData,
            result,
            cancellationToken);
    }

    private void UpdateEntities(
        DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker
                     .Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:

                    entry.Entity.CreatedAt =
                        DateTime.UtcNow;

                    entry.Entity.CreatedBy =
                        _currentUserService.Email;

                    break;

                case EntityState.Modified:

                    entry.Entity.UpdatedAt =
                        DateTime.UtcNow;

                    entry.Entity.UpdatedBy =
                        _currentUserService.Email;

                    break;

                case EntityState.Deleted:

                    entry.State =
                        EntityState.Modified;

                    entry.Entity.IsDeleted = true;

                    entry.Entity.DeletedAt =
                        DateTime.UtcNow;

                    entry.Entity.DeletedBy =
                        _currentUserService.Email;

                    break;
            }
        }
    }
}
