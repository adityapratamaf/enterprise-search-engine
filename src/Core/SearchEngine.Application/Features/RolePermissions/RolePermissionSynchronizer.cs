using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Common.Security;
using SearchEngine.Application.Features.RolePermissions.DTOs;
using SearchEngine.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SearchEngine.Application.Common.Exceptions;

namespace SearchEngine.Application.Features.RolePermissions;

/// <summary>
/// Menyinkronkan permission sebuah role untuk satu module berdasarkan
/// daftar ActionId yang diinginkan: menghapus grant yang tidak lagi diminta
/// dan menambahkan grant baru — tanpa menghapus-lalu-membuat-ulang semuanya.
/// Perubahan Remove + Add dieksekusi dalam satu SaveChanges (satu transaksi
/// atomik oleh EF Core), sehingga tetap konsisten.
/// </summary>
internal static class RolePermissionSynchronizer
{
    public static async Task<Result<bool>> SyncAsync(
        IApplicationIdentityDbContext context,
        RoleManager<IdentityRole> roleManager,
        IAuditActionService auditActionService,
        AssignModulePermissionRequest request,
        CancellationToken cancellationToken)
    {
        var role =
            await roleManager.FindByNameAsync(request.RoleName);

        if (role is null)
        {
            throw new NotFoundException("Role not found");
        }

        // Permission (master) yang diinginkan = module × action yang diminta.
        var desiredPermissionIds =
            await context.Permissions
                .AsNoTracking()
                .Where(p =>
                    p.ModuleId == request.ModuleId &&
                    request.ActionIds.Contains(p.PermissionActionId) &&
                    p.IsActive)
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

        // Grant yang sudah ada untuk role ini pada module tsb (tracked).
        var existing =
            await context.RolePermissions
                .Where(rp =>
                    rp.RoleId == role.Id &&
                    rp.Permission.ModuleId == request.ModuleId)
                .ToListAsync(cancellationToken);

        var desiredSet = desiredPermissionIds.ToHashSet();
        var existingSet = existing.Select(x => x.PermissionId).ToHashSet();

        var toRemove =
            existing
                .Where(x => !desiredSet.Contains(x.PermissionId))
                .ToList();

        var toAdd =
            desiredPermissionIds
                .Where(pid => !existingSet.Contains(pid))
                .Select(pid => new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = role.Id,
                    PermissionId = pid
                })
                .ToList();

        if (toRemove.Count == 0 && toAdd.Count == 0)
        {
            return Result<bool>
                .SuccessResult(true, "No permission changes");
        }

        if (toRemove.Count > 0)
        {
            context.RolePermissions.RemoveRange(toRemove);
        }

        if (toAdd.Count > 0)
        {
            await context.RolePermissions.AddRangeAsync(
                toAdd,
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);

        await auditActionService.LogAsync(
            action: SecurityAuditAction.PermissionChanged,
            module: SecurityAuditAction.Module,
            tableName: "RolePermissions",
            recordId: role.Id,
            newValues: new
            {
                request.RoleName,
                request.ModuleId,
                request.ActionIds,
                Added = toAdd.Count,
                Removed = toRemove.Count
            },
            cancellationToken: cancellationToken);

        return Result<bool>
            .SuccessResult(true, "Permissions updated successfully");
    }
}
