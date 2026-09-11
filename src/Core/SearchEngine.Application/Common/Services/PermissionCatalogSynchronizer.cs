using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Application.Common.Services;

/// <summary>
/// Menjaga Permission sebagai GENERATED CATALOG = kombinasi setiap Module
/// dengan setiap PermissionAction yang aktif (mis. "users.view").
///
/// Idempoten: hanya menambahkan kombinasi yang belum ada, tidak pernah
/// menghapus atau mengelola Permission secara manual. Dipakai bersama oleh
/// seeder startup, CreateModule, dan CreatePermissionAction.
/// </summary>
public static class PermissionCatalogSynchronizer
{
    public static async Task<int> SyncAsync(
        IApplicationIdentityDbContext context,
        CancellationToken cancellationToken = default)
    {
        var modules =
            await context.Modules
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        var actions =
            await context.PermissionActions
                .AsNoTracking()
                .Where(x => x.IsActive)
                .ToListAsync(cancellationToken);

        if (modules.Count == 0 || actions.Count == 0)
        {
            return 0;
        }

        var existingCodes =
            (await context.Permissions
                .Select(x => x.Code)
                .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var toAdd = new List<Permission>();

        foreach (var module in modules)
        {
            foreach (var action in actions)
            {
                var code =
                    $"{module.ModuleId}.{action.Code}"
                        .ToLowerInvariant();

                if (existingCodes.Contains(code))
                {
                    continue;
                }

                toAdd.Add(new Permission
                {
                    Id = Guid.NewGuid(),
                    ModuleId = module.Id,
                    PermissionActionId = action.Id,
                    Code = code,
                    IsActive = true
                });

                existingCodes.Add(code);
            }
        }

        if (toAdd.Count == 0)
        {
            return 0;
        }

        await context.Permissions.AddRangeAsync(
            toAdd,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return toAdd.Count;
    }
}
