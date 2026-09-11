using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Roles.DTOs;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Application.Features.Roles.Queries.GetAllRoles;

public class GetAllRolesHandler
    : IRequestHandler<
        GetAllRolesQuery,
        Result<List<ReadRoleResponse>>>
{
    private readonly RoleManager<IdentityRole>
        _roleManager;

    public GetAllRolesHandler(
        RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<Result<List<ReadRoleResponse>>> Handle(
        GetAllRolesQuery request,
        CancellationToken cancellationToken)
    {
        var roles = await _roleManager.Roles
            .AsNoTracking()
            .ProjectToType<ReadRoleResponse>()
            .ToListAsync(cancellationToken);

        return Result<List<ReadRoleResponse>>
            .SuccessResult(roles);
    }
}
