using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Common.Security;
using MediatR;

namespace SearchEngine.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenHandler
    : IRequestHandler<
        RefreshTokenCommand,
        Result<AuthResponse>>
{
    private readonly IIdentityService
        _identityService;

    private readonly IAuditActionService
        _auditActionService;

    public RefreshTokenHandler(
        IIdentityService identityService,
        IAuditActionService auditActionService)
    {
        _identityService = identityService;
        _auditActionService = auditActionService;
    }

    public async Task<Result<AuthResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _identityService
            .RefreshTokenAsync(
                request.RefreshToken);

        if (result.Success)
        {
            // Tidak pernah mencatat nilai refresh token (data sensitif).
            await _auditActionService.LogAsync(
                action: SecurityAuditAction.RefreshTokenSuccess,
                module: SecurityAuditAction.Module,
                tableName: "RefreshTokens",
                recordId: result.Data?.User.Id ?? "-",
                cancellationToken: cancellationToken);
        }
        else
        {
            await _auditActionService.LogAsync(
                action: SecurityAuditAction.RefreshTokenFailed,
                module: SecurityAuditAction.Module,
                tableName: "RefreshTokens",
                recordId: "-",
                newValues: new { result.Message },
                cancellationToken: cancellationToken);
        }

        return result;
    }
}
