using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Common.Security;
using SearchEngine.Application.Features.Roles.DTOs;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SearchEngine.Application.Common.Exceptions;

namespace SearchEngine.Application.Features.Roles.Commands.CreateRole;

public class CreateRoleHandler
    : IRequestHandler<
        CreateRoleCommand,
        Result<ReadRoleResponse>>
{
    private readonly RoleManager<IdentityRole>
        _roleManager;

    private readonly IAuditActionService
        _auditActionService;

    public CreateRoleHandler(
        RoleManager<IdentityRole> roleManager,
        IAuditActionService auditActionService)
    {
        _roleManager = roleManager;
        _auditActionService = auditActionService;
    }

    public async Task<Result<ReadRoleResponse>> Handle(
        CreateRoleCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await _roleManager
            .RoleExistsAsync(
                request.Request.Name!);

        if (exists)
        {
            throw new ConflictException("Role already exists");
        }

        var role = new IdentityRole
        {
            Name = request.Request.Name
        };

        var result = await _roleManager
            .CreateAsync(role);

        if (!result.Succeeded)
        {
            return Result<ReadRoleResponse>
                .Failure(
                    "Failed create role",
                    result.Errors);
        }

        // Model dinamis: role baru tidak memiliki grant apa pun.
        // Matriks permission diturunkan dari katalog saat di-query,
        // sehingga tidak perlu pre-seed baris permission.

        await _auditActionService.LogAsync(
            action: SecurityAuditAction.RoleChanged,
            module: SecurityAuditAction.Module,
            tableName: "AspNetRoles",
            recordId: role.Id,
            newValues: new { role.Id, role.Name },
            cancellationToken: cancellationToken);

        return Result<ReadRoleResponse>
            .SuccessResult(
                role.Adapt<ReadRoleResponse>(),
                "Role created successfully");
    }
}
