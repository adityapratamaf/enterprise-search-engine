using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Users.DTOs;

using MediatR;

namespace SearchEngine.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserHandler
    : IRequestHandler<
        UpdateUserCommand,
        Result<ReadUserResponse>>
{
    private readonly IIdentityService
        _identityService;

    private readonly IAuditActionService
        _auditActionService;

    public UpdateUserHandler(
        IIdentityService identityService,
        IAuditActionService auditLogService)
    {
        _identityService = identityService;

        _auditActionService = auditLogService;
    }

    public async Task<Result<ReadUserResponse>>
        Handle(
            UpdateUserCommand request,
            CancellationToken cancellationToken)
    {
        // OLD VALUES
        var oldUser = await _identityService
            .GetUserByIdAsync(
                request.Id);

        var result = await _identityService
            .UpdateUserAsync(
                request.Id,
                request.Request);

        if (!result.Success || result.Data is null)
        {
            return result;
        }

        await _auditActionService.LogAsync(
            action: "UPDATE",
            module: "Users",
            tableName: "AspNetUsers",
            recordId: request.Id,
            oldValues: oldUser.Data,
            newValues: result.Data,
            cancellationToken:
                cancellationToken);

        return result;
    }
}
