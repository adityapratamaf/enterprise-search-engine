using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SearchEngine.Application.Common.Exceptions;

namespace SearchEngine.Application.Features.RolePermissions.Commands.RemoveModulePermission;

public class RemoveModulePermissionCommandHandler
    : IRequestHandler<
        RemoveModulePermissionCommand,
        Result<bool>>
{
    private readonly IApplicationIdentityDbContext _context;

    private readonly RoleManager<IdentityRole> _roleManager;

    private readonly IAuditActionService _auditActionService;

    public RemoveModulePermissionCommandHandler(
        IApplicationIdentityDbContext context,
        RoleManager<IdentityRole> roleManager,
        IAuditActionService auditActionService)
    {
        _context = context;
        _roleManager = roleManager;
        _auditActionService = auditActionService;
    }

    public async Task<Result<bool>> Handle(
        RemoveModulePermissionCommand request,
        CancellationToken cancellationToken)
    {
        var role =
            await _roleManager.FindByNameAsync(request.RoleName);

        if (role is null)
        {
            throw new NotFoundException("Role not found");
        }

        var toRemove =
            await _context.RolePermissions
                .Where(rp =>
                    rp.RoleId == role.Id &&
                    rp.Permission.ModuleId == request.ModuleId)
                .ToListAsync(cancellationToken);

        if (toRemove.Count == 0)
        {
            return Result<bool>
                .SuccessResult(true, "No permissions to remove");
        }

        _context.RolePermissions.RemoveRange(toRemove);

        await _context.SaveChangesAsync(cancellationToken);

        await _auditActionService.LogAsync(
            action: SecurityAuditAction.PermissionChanged,
            module: SecurityAuditAction.Module,
            tableName: "RolePermissions",
            recordId: role.Id,
            oldValues: new
            {
                request.RoleName,
                request.ModuleId,
                Removed = toRemove.Count
            },
            cancellationToken: cancellationToken);

        return Result<bool>
            .SuccessResult(true);
    }
}
