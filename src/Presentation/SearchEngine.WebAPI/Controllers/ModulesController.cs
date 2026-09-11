using SearchEngine.Application.Common.Security;
using SearchEngine.Application.Features.Modules.Commands.CreateModule;
using SearchEngine.Application.Features.Modules.DTOs;
using SearchEngine.Application.Features.Modules.Queries.GetAllModules;
using SearchEngine.Application.Features.Modules.Commands.DeleteModule;
using SearchEngine.Application.Features.Modules.Commands.UpdateModule;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SearchEngine.WebAPI.Controllers;

[ApiController]
[EnableRateLimiting("api")]
[Route("api/modules")]
[Authorize]
public class ModulesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ModulesController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission("modules", "view")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(
            new GetAllModulesQuery());

        return Ok(result);
    }

    [HttpPost]
    [HasPermission("modules", "create")]
    public async Task<IActionResult> Create(
        CreateModuleRequest request)
    {
        var result = await _mediator.Send(
            new CreateModuleCommand(request));

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [HasPermission("modules", "update")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateModuleRequest request)
    {
        var result =
            await _mediator.Send(
                new UpdateModuleCommand(
                    id,
                    request));

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("modules", "delete")]
    public async Task<IActionResult> Delete(
        Guid id)
    {
        var result =
            await _mediator.Send(
                new DeleteModuleCommand(
                    id));

        return Ok(result);
    }
}
