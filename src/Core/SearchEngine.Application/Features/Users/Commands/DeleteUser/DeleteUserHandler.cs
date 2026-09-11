using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;

using MediatR;

namespace SearchEngine.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserHandler
    : IRequestHandler<
        DeleteUserCommand,
        Result<string>>
{
    private readonly IIdentityService
        _identityService;

    private readonly IAuditActionService
        _auditActionService;

    public DeleteUserHandler(
        IIdentityService identityService,
        IAuditActionService auditLogService)
    {
        _identityService = identityService;

        _auditActionService = auditLogService;
    }

    public async Task<Result<string>> Handle(
        DeleteUserCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _identityService
            .DeleteUserAsync(
                request.Id);

        if (!result.Success)
        {
            return result;
        }

        await _auditActionService.LogAsync(
            action: "DELETE",
            module: "Users",
            tableName: "AspNetUsers",
            recordId: request.Id,
            oldValues: new
            {
                UserId = request.Id
            },
            cancellationToken:
                cancellationToken);

        return result;
    }
}
