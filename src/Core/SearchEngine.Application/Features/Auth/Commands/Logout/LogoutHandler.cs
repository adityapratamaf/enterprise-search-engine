using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Common.Security;
using MediatR;
using SearchEngine.Application.Common.Exceptions;

namespace SearchEngine.Application.Features.Auth.Commands.Logout;

public class LogoutHandler
    : IRequestHandler<
        LogoutCommand,
        Result<string>>
{
    private readonly IIdentityService
        _identityService;

    private readonly ICurrentUserService
        _currentUserService;

    private readonly IAuditActionService
        _auditActionService;

    public LogoutHandler(
        IIdentityService identityService,
        ICurrentUserService currentUserService,
        IAuditActionService auditActionService)
    {
        _identityService = identityService;
        _currentUserService = currentUserService;
        _auditActionService = auditActionService;
    }

    public async Task<Result<string>> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        var userId =
            _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedException("Unauthorized");
        }

        var result = await _identityService
            .LogoutAsync(userId);

        if (result.Success)
        {
            await _auditActionService.LogAsync(
                action: SecurityAuditAction.Logout,
                module: SecurityAuditAction.Module,
                tableName: "RefreshTokens",
                recordId: userId,
                cancellationToken: cancellationToken);
        }

        return result;
    }
}
