using SearchEngine.Application.Common.Security;
using SearchEngine.Application.Features.Users.Commands.CreateUser;
using SearchEngine.Application.Features.Users.DTOs;
using SearchEngine.Application.Features.Users.Queries.GetAllUsers;
using SearchEngine.Application.Features.Users.Commands.DeleteUser;
using SearchEngine.Application.Features.Users.Commands.UpdateUser;
using SearchEngine.Application.Features.Users.Queries.GetUserById;
using SearchEngine.Application.Features.Users.Commands.ToggleUserStatus;
using SearchEngine.Application.Common.Models;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SearchEngine.WebAPI.Controllers;

[ApiController]
[EnableRateLimiting("api")]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission("users", "view")]
    public async Task<IActionResult> GetAll(
        [FromQuery]
        PaginationRequest request)
    {
        var result = await _mediator.Send(
            new GetAllUsersQuery(request));

        return Ok(result);
    }

    [HttpGet("{id}")]
    [HasPermission("users", "view")]
    public async Task<IActionResult> GetById(
        string id)
    {
        var result = await _mediator.Send(
            new GetUserByIdQuery(id));

        return Ok(result);
    }

    [HttpPost]
    [HasPermission("users", "create")]
    public async Task<IActionResult> Create(
        CreateUserRequest request)
    {
        var result = await _mediator.Send(
            new CreateUserCommand(request));

        return Ok(result);
    }

    [HttpPut("{id}")]
    [HasPermission("users", "update")]
    public async Task<IActionResult> Update(
        string id,
        UpdateUserRequest request)
    {
        var result = await _mediator.Send(
            new UpdateUserCommand(
                id,
                request));

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [HasPermission("users", "delete")]
    public async Task<IActionResult> Delete(
        string id)
    {
        var result = await _mediator.Send(
            new DeleteUserCommand(id));

        return Ok(result);
    }

    [HttpPatch("{id}/toggle-status")]
    [HasPermission("users", "update")]
    public async Task<IActionResult> ToggleStatus(
        string id)
    {
        var result = await _mediator.Send(
            new ToggleUserStatusCommand(id));

        return Ok(result);
    }

}
