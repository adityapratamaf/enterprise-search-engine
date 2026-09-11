using SearchEngine.Infrastructure.Identity.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Infrastructure.Identity.Authorization;

public class PermissionHandler
    : AuthorizationHandler<PermissionRequirement>
{
    private readonly ApplicationIdentityDbContext _context;

    public PermissionHandler(
        ApplicationIdentityDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var roleNames = context.User.Claims
            .Where(x => x.Type.Contains("role"))
            .Select(x => x.Value)
            .ToList();

        if (roleNames.Count == 0)
        {
            return;
        }

        var moduleCode = requirement.Module.ToLower();
        var actionCode = requirement.Action.ToLower();

        // Query efisien: EXISTS (AnyAsync) tanpa memuat entity,
        // AsNoTracking, dan pencocokan code case-insensitive.
        var hasPermission = await (
            from rp in _context.RolePermissions.AsNoTracking()
            join role in _context.Roles
                on rp.RoleId equals role.Id
            where roleNames.Contains(role.Name!)
                && rp.Permission.IsActive
                && rp.Permission.Module.ModuleId.ToLower() == moduleCode
                && rp.Permission.PermissionAction.Code.ToLower() == actionCode
            select rp.Id)
            .AnyAsync();

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
