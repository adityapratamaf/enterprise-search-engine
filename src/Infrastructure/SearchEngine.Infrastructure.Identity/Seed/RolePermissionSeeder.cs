using SearchEngine.Domain.Entities;
using SearchEngine.Infrastructure.Identity.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Infrastructure.Identity.Seed;

/// <summary>
/// Memberi role SuperAdmin seluruh Permission (via RolePermission dengan RoleId).
/// Idempoten; dijalankan setelah katalog Permission ter-seed.
/// </summary>
public static class RolePermissionSeeder
{
    public static async Task SeedAsync(
        ApplicationIdentityDbContext context,
        RoleManager<IdentityRole> roleManager)
    {
        var superAdmin =
            await roleManager.FindByNameAsync("SuperAdmin");

        if (superAdmin is null)
        {
            return;
        }

        var permissionIds =
            await context.Permissions
                .AsNoTracking()
                .Select(x => x.Id)
                .ToListAsync();

        if (permissionIds.Count == 0)
        {
            return;
        }

        var granted =
            (await context.RolePermissions
                .Where(x => x.RoleId == superAdmin.Id)
                .Select(x => x.PermissionId)
                .ToListAsync())
            .ToHashSet();

        var toAdd =
            permissionIds
                .Where(pid => !granted.Contains(pid))
                .Select(pid => new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = superAdmin.Id,
                    PermissionId = pid
                })
                .ToList();

        if (toAdd.Count == 0)
        {
            return;
        }

        context.RolePermissions.AddRange(toAdd);

        await context.SaveChangesAsync();
    }
}
