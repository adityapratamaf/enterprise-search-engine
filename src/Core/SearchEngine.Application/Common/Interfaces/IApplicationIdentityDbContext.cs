using SearchEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Application.Common.Interfaces;

public interface IApplicationIdentityDbContext
{
    DbSet<Module> Modules { get; }

    DbSet<PermissionAction> PermissionActions { get; }

    DbSet<Permission> Permissions { get; }

    DbSet<RolePermission> RolePermissions { get; }

    DbSet<AuditAction> AuditLogs { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken);
}
