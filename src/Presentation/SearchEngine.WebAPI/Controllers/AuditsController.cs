using SearchEngine.Application.Common.Security;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Audit.Queries.GetAllAuditAction;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SearchEngine.WebAPI.Controllers;

[ApiController]
[EnableRateLimiting("api")]
[Route("api/audit")]
[Authorize]
public class AuditsController : ControllerBase
{
    private readonly IMediator
        _mediator;

    public AuditsController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("action")]
    [HasPermission("audits", "view")]
    public async Task<IActionResult> GetActions(
        [FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(
            new GetAllAuditActionQuery(
                request));

        return Ok(result);
    }
}
