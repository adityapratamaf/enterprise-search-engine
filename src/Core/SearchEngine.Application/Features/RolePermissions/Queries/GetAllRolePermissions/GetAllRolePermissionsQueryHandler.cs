using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.RolePermissions.DTOs;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SearchEngine.Application.Common.Exceptions;

namespace SearchEngine.Application.Features.RolePermissions.Queries.GetAllRolePermissions;

public class GetAllRolePermissionsQueryHandler
    : IRequestHandler<
        GetAllRolePermissionsQuery,
        Result<List<ReadRolePermissionResponse>>>
{
    private readonly IApplicationIdentityDbContext _context;

    private readonly RoleManager<IdentityRole> _roleManager;

    public GetAllRolePermissionsQueryHandler(
        IApplicationIdentityDbContext context,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _roleManager = roleManager;
    }

    public async Task<Result<List<ReadRolePermissionResponse>>> Handle(
        GetAllRolePermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var role =
            await _roleManager.FindByNameAsync(request.RoleName);

        if (role is null)
        {
            throw new NotFoundException("Role not found");
        }

        // Katalog master permission (module + action).
        var catalog =
            await _context.Permissions
                .AsNoTracking()
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    p.Id,
                    p.ModuleId,
                    ModuleName = p.Module.ModuleName,
                    p.PermissionActionId,
                    ActionName = p.PermissionAction.Name,
                    ActionOrder = p.PermissionAction.DisplayOrder
                })
                .ToListAsync(cancellationToken);

        // Permission yang sudah di-grant untuk role ini.
        var grantedPermissionIds =
            (await _context.RolePermissions
                .AsNoTracking()
                .Where(rp => rp.RoleId == role.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync(cancellationToken))
            .ToHashSet();

        var response =
            catalog
                .GroupBy(x => new { x.ModuleId, x.ModuleName })
                .OrderBy(g => g.Key.ModuleName)
                .Select(g => new ReadRolePermissionResponse
                {
                    ModuleId = g.Key.ModuleId,
                    ModuleName = g.Key.ModuleName,
                    Permissions = g
                        .OrderBy(x => x.ActionOrder)
                        .Select(x => new RolePermissionActionResponse
                        {
                            ActionId = x.PermissionActionId,
                            ActionName = x.ActionName,
                            Granted = grantedPermissionIds.Contains(x.Id)
                        })
                        .ToList()
                })
                .ToList();

        return Result<List<ReadRolePermissionResponse>>
            .SuccessResult(response);
    }
}
