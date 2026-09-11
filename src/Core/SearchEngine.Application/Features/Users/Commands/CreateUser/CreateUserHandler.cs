using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Users.DTOs;

using MediatR;

namespace SearchEngine.Application.Features.Users.Commands.CreateUser;

public class CreateUserHandler
    : IRequestHandler<
        CreateUserCommand,
        Result<ReadUserResponse>>
{
    private readonly IIdentityService
        _identityService;

    private readonly IAuditActionService
        _auditActionService;

    public CreateUserHandler(
        IIdentityService identityService,
        IAuditActionService auditLogService)
    {
        _identityService = identityService;
        _auditActionService = auditLogService;
    }

    public async Task<Result<ReadUserResponse>> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _identityService
            .CreateUserAsync(
                request.Request);

        if (!result.Success || result.Data is null)
        {
            return result;
        }

        // Audit Action
        await _auditActionService.LogAsync(
            action: "CREATE",
            module: "Users",
            tableName: "AspNetUsers",
            recordId: result.Data.Id,
            newValues: result.Data);

        return result;
    }
}
