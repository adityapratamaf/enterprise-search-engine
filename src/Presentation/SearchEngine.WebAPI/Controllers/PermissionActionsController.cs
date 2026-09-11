using SearchEngine.Application.Common.Security;
using SearchEngine.Application.Features.PermissionActions.Commands.CreatePermissionAction;
using SearchEngine.Application.Features.PermissionActions.Commands.DeletePermissionAction;
using SearchEngine.Application.Features.PermissionActions.Commands.UpdatePermissionAction;
using SearchEngine.Application.Features.PermissionActions.DTOs;
using SearchEngine.Application.Features.PermissionActions.Queries.GetAllPermissionActions;
using SearchEngine.Application.Features.PermissionActions.Queries.GetPermissionActionById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SearchEngine.WebAPI.Controllers;

[ApiController]
[Authorize]
[EnableRateLimiting("api")]
[Route("api/permission-actions")]
public class PermissionActionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PermissionActionsController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission("permission-actions", "view")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(
            new GetAllPermissionActionsQuery());

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission("permission-actions", "view")]
    public async Task<IActionResult> GetById(
        Guid id)
    {
        var result = await _mediator.Send(
            new GetPermissionActionByIdQuery(id));

        return Ok(result);
    }

    [HttpPost]
    [HasPermission("permission-actions", "create")]
    public async Task<IActionResult> Create(
        [FromBody] CreatePermissionActionRequest request)
    {
        var result = await _mediator.Send(
            new CreatePermissionActionCommand(request));

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("permission-actions", "update")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdatePermissionActionRequest request)
    {
        var result = await _mediator.Send(
            new UpdatePermissionActionCommand(id, request));

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("permission-actions", "delete")]
    public async Task<IActionResult> Delete(
        Guid id)
    {
        var result = await _mediator.Send(
            new DeletePermissionActionCommand(id));

        return Ok(result);
    }
}
