using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SearchEngine.Application.Common.Exceptions;

namespace SearchEngine.Application.Features.Roles.Commands.UpdateRole;

public class UpdateRoleHandler
    : IRequestHandler<
        UpdateRoleCommand,
        Result<bool>>
{
    private readonly RoleManager<IdentityRole>
        _roleManager;

    private readonly IAuditActionService
        _auditActionService;

    public UpdateRoleHandler(
        RoleManager<IdentityRole> roleManager,
        IAuditActionService auditActionService)
    {
        _roleManager = roleManager;
        _auditActionService = auditActionService;
    }

    public async Task<Result<bool>> Handle(
        UpdateRoleCommand request,
        CancellationToken cancellationToken)
    {
        var role =
            await _roleManager.FindByIdAsync(
                request.Id);

        if (role is null)
        {
            throw new NotFoundException("Role not found");
        }

        role.Name =
            request.Request.Name;

        await _roleManager.UpdateAsync(role);

        await _auditActionService.LogAsync(
            action: SecurityAuditAction.RoleChanged,
            module: SecurityAuditAction.Module,
            tableName: "AspNetRoles",
            recordId: role.Id,
            newValues: new { role.Id, role.Name },
            cancellationToken: cancellationToken);

        return Result<bool>
            .SuccessResult(
                true,
                "Role updated successfully");
    }
}
