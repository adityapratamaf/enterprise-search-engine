using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Auth.DTOs;
using SearchEngine.Infrastructure.Identity.Context;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Infrastructure.Identity.Services;

/// <summary>
/// Satu-satunya tempat pembacaan permission efektif user, bersumber dari
/// model dinamis: RoleId -> RolePermission -> Permission -> Module + Action.
/// </summary>
public class PermissionService
{
    private readonly ApplicationIdentityDbContext
        _context;

    public PermissionService(
        ApplicationIdentityDbContext context)
    {
        _context = context;
    }

    // ========== Menus & Permissions (untuk /api/auth/profile) ==========
    public async Task<(
        List<ReadMenuResponse> Menus,
        List<ReadPermissionAuthResponse> Permissions)>
        GetMenusAndPermissionsAsync(
            IList<string> roles)
    {
        var byModule =
            (await GetGrantsAsync(roles))
                .GroupBy(x => x.ModuleCode)
                .ToList();

        var permissions =
            byModule
                .Select(g => new ReadPermissionAuthResponse
                {
                    ModuleId = g.Key,
                    ModuleName = g.First().ModuleName,
                    Actions = MapActions(g)
                })
                .ToList();

        var menus =
            byModule
                .Where(HasViewAction)
                .Select(g => new ReadMenuResponse
                {
                    ModuleId = g.Key,
                    ModuleName = g.First().ModuleName,
                    ModulePath = g.First().ModulePath
                })
                .ToList();

        return (menus, permissions);
    }

    // ========== Module Permissions (untuk login/AuthResponse) ==========
    public async Task<List<ModulePermissionResponse>>
        GetModulePermissionsAsync(
            IList<string> roles)
    {
        return (await GetGrantsAsync(roles))
            .GroupBy(x => x.ModuleCode)
            .Select(g => new ModulePermissionResponse
            {
                ModuleId = g.Key,
                ModuleName = g.First().ModuleName,
                ModulePath = g.First().ModulePath,
                Actions = MapActions(g)
            })
            .ToList();
    }

    // ========== Core query (dipakai bersama) ==========
    private async Task<List<PermissionGrant>> GetGrantsAsync(
        IList<string> roles)
    {
        return await (
            from rp in _context.RolePermissions.AsNoTracking()
            join role in _context.Roles
                on rp.RoleId equals role.Id
            where roles.Contains(role.Name!)
                && rp.Permission.IsActive
            select new PermissionGrant
            {
                ModuleCode = rp.Permission.Module.ModuleId,
                ModuleName = rp.Permission.Module.ModuleName,
                ModulePath = rp.Permission.Module.ModulePath,
                ActionCode = rp.Permission.PermissionAction.Code
            })
            .ToListAsync();
    }

    private static List<string> MapActions(
        IEnumerable<PermissionGrant> grants)
    {
        return grants
            .Select(x => x.ActionCode.ToLowerInvariant())
            .Distinct()
            .OrderBy(x => x)
            .ToList();
    }

    private static bool HasViewAction(
        IEnumerable<PermissionGrant> grants)
    {
        return grants.Any(x =>
            string.Equals(
                x.ActionCode,
                "View",
                StringComparison.OrdinalIgnoreCase));
    }

    private sealed class PermissionGrant
    {
        public string ModuleCode { get; set; } = default!;
        public string ModuleName { get; set; } = default!;
        public string ModulePath { get; set; } = default!;
        public string ActionCode { get; set; } = default!;
    }
}
