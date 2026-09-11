using SearchEngine.Application.Common.Services;
using SearchEngine.Infrastructure.Identity.Context;

namespace SearchEngine.Infrastructure.Identity.Seed;

/// <summary>
/// Startup seeding untuk katalog Permission. Delegasikan ke
/// <see cref="PermissionCatalogSynchronizer"/> agar logika generate katalog
/// (Module x PermissionAction aktif) konsisten di seluruh sistem.
/// </summary>
public static class PermissionCatalogSeeder
{
    public static async Task SeedAsync(
        ApplicationIdentityDbContext context)
    {
        await PermissionCatalogSynchronizer.SyncAsync(context);
    }
}
