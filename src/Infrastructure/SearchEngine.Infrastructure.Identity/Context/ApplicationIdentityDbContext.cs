using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Domain.Entities;
using SearchEngine.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Infrastructure.Identity.Context;

public class ApplicationIdentityDbContext
    : IdentityDbContext<ApplicationUser>,
      IApplicationIdentityDbContext
{
    public ApplicationIdentityDbContext(
        DbContextOptions<ApplicationIdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<Module> Modules
        => Set<Module>();

    public DbSet<PermissionAction> PermissionActions
        => Set<PermissionAction>();

    public DbSet<Permission> Permissions
        => Set<Permission>();

    public DbSet<RolePermission> RolePermissions
        => Set<RolePermission>();

    public DbSet<RefreshToken> RefreshTokens
        => Set<RefreshToken>();

    public DbSet<AuditAction> AuditLogs
        => Set<AuditAction>();

    protected override void OnModelCreating(
        ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationIdentityDbContext).Assembly);
    }
}
