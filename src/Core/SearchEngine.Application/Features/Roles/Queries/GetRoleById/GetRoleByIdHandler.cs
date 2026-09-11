using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Roles.DTOs;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SearchEngine.Application.Common.Exceptions;

namespace SearchEngine.Application.Features.Roles.Queries.GetRoleById;

public class GetRoleByIdHandler
    : IRequestHandler<
        GetRoleByIdQuery,
        Result<ReadRoleResponse>>
{
    private readonly RoleManager<IdentityRole>
        _roleManager;

    public GetRoleByIdHandler(
        RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<Result<ReadRoleResponse>> Handle(
        GetRoleByIdQuery request,
        CancellationToken cancellationToken)
    {
        var role = await _roleManager
            .FindByIdAsync(request.Id);

        if (role is null)
        {
            throw new NotFoundException("Role not found");
        }

        return Result<ReadRoleResponse>
            .SuccessResult(
                role.Adapt<ReadRoleResponse>());
    }
}
