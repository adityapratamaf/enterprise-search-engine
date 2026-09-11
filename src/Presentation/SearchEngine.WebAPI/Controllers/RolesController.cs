using SearchEngine.Application.Common.Security;
using SearchEngine.Application.Features.Roles.Commands.CreateRole;
using SearchEngine.Application.Features.Roles.DTOs;
using SearchEngine.Application.Features.Roles.Queries.GetAllRoles;
using SearchEngine.Application.Features.Roles.Queries.GetRoleById;
using SearchEngine.Application.Features.Roles.Commands.UpdateRole;
using SearchEngine.Application.Features.Roles.Commands.DeleteRole;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SearchEngine.WebAPI.Controllers;

[ApiController]
[EnableRateLimiting("api")]
[Route("api/roles")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolesController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission("roles", "view")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(
            new GetAllRolesQuery());

        return Ok(result);
    }

    [HttpGet("{id}")]
    [HasPermission("roles", "view")]
    public async Task<IActionResult> GetById(
        string id)
    {
        var result = await _mediator.Send(
            new GetRoleByIdQuery(id));

        return Ok(result);
    }

    [HttpPost]
    [HasPermission("roles", "create")]
    public async Task<IActionResult> Create(
        CreateRoleRequest request)
    {
        var result = await _mediator.Send(
            new CreateRoleCommand(request));

        return Ok(result);
    }

    [HttpPut("{id}")]
    [HasPermission("roles", "update")]
    public async Task<IActionResult> Update(
        string id,
        UpdateRoleRequest request)
    {
        var result =
            await _mediator.Send(
                new UpdateRoleCommand(
                    id,
                    request));

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [HasPermission("roles", "delete")]
    public async Task<IActionResult> Delete(
        string id)
    {
        var result =
            await _mediator.Send(
                new DeleteRoleCommand(
                    id));

        return Ok(result);
    }

}
