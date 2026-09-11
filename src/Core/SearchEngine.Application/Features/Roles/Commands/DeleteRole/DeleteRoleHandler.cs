using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SearchEngine.Application.Common.Exceptions;

namespace SearchEngine.Application.Features.Roles.Commands.DeleteRole;

public class DeleteRoleHandler
    : IRequestHandler<
        DeleteRoleCommand,
        Result<bool>>
{
    private readonly RoleManager<IdentityRole>
        _roleManager;

    private readonly IApplicationIdentityDbContext
        _context;

    private readonly IAuditActionService
        _auditActionService;

    public DeleteRoleHandler(
        RoleManager<IdentityRole> roleManager,
        IApplicationIdentityDbContext context,
        IAuditActionService auditActionService)
    {
        _roleManager = roleManager;
        _context = context;
        _auditActionService = auditActionService;
    }

    public async Task<Result<bool>> Handle(
        DeleteRoleCommand request,
        CancellationToken cancellationToken)
    {
        var role =
            await _roleManager.FindByIdAsync(
                request.Id);

        if (role is null)
        {
            throw new NotFoundException("Role not found");
        }

        // Hapus grant (RolePermission) role ini lebih dulu karena FK
        // RolePermission -> AspNetRoles diset Restrict.
        var grants =
            await _context
                .RolePermissions
                .Where(rp =>
                    rp.RoleId ==
                    role.Id)
                .ToListAsync(
                    cancellationToken);

        _context
            .RolePermissions
            .RemoveRange(
                grants);

        await _context
            .SaveChangesAsync(
                cancellationToken);

        await _roleManager
            .DeleteAsync(role);

        await _auditActionService.LogAsync(
            action: SecurityAuditAction.RoleChanged,
            module: SecurityAuditAction.Module,
            tableName: "AspNetRoles",
            recordId: role.Id,
            oldValues: new { role.Id, role.Name },
            cancellationToken: cancellationToken);

        return Result<bool>
            .SuccessResult(
                true,
                "Role deleted successfully");
    }
}
