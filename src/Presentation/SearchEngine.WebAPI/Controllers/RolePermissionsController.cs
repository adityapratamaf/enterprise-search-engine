using SearchEngine.Application.Features.RolePermissions.DTOs;
using SearchEngine.Application.Features.RolePermissions.Commands.UpdateModulePermission;
using SearchEngine.Application.Features.RolePermissions.Commands.AssignModulePermission;
using SearchEngine.Application.Features.RolePermissions.Queries.GetAllRolePermissions;
using SearchEngine.Application.Features.RolePermissions.Commands.RemoveModulePermission;
using SearchEngine.Application.Common.Security;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SearchEngine.WebAPI.Controllers;

[ApiController]
[EnableRateLimiting("api")]
[Route("api/roles")]
[Authorize]
public class RolePermissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolePermissionsController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{roleName}/permissions")]
    [HasPermission("roles", "view")]
    public async Task<IActionResult> GetPermissions(
        string roleName)
    {
        var result =
            await _mediator.Send(
                new GetAllRolePermissionsQuery(
                    roleName));

        return Ok(result);
    }

    [HttpPost("{roleName}/permissions")]
    [HasPermission("roles", "update")]
    public async Task<IActionResult> Assign(
        string roleName,
        AssignModulePermissionRequest request)
    {
        request.RoleName = roleName;

        var result =
            await _mediator.Send(
                new AssignModulePermissionCommand(
                    request));

        return Ok(result);
    }

    [HttpPut("{roleName}/permissions")]
    [HasPermission("roles", "update")]
    public async Task<IActionResult> Update(
        string roleName,
        AssignModulePermissionRequest request)
    {
        request.RoleName = roleName;

        var result =
            await _mediator.Send(
                new UpdateModulePermissionCommand(
                    request));

        return Ok(result);
    }

    [HttpDelete("{roleName}/permissions/{moduleId:guid}")]
    [HasPermission("roles", "update")]
    public async Task<IActionResult> Delete(
        string roleName,
        Guid moduleId)
    {
        var result =
            await _mediator.Send(
                new RemoveModulePermissionCommand(
                    roleName,
                    moduleId));

        return Ok(result);
    }
}
