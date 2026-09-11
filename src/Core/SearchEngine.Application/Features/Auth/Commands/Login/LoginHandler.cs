using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Common.Security;
using MediatR;

namespace SearchEngine.Application.Features.Auth.Commands.Login;

public class LoginHandler
    : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IIdentityService
        _identityService;

    private readonly IAuditActionService
        _auditActionService;

    public LoginHandler(
        IIdentityService identityService,
        IAuditActionService auditActionService)
    {
        _identityService = identityService;
        _auditActionService = auditActionService;
    }

    public async Task<Result<AuthResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _identityService
            .LoginAsync(
                request.Email,
                request.Password);

        if (result.Success)
        {
            await _auditActionService.LogAsync(
                action: SecurityAuditAction.LoginSuccess,
                module: SecurityAuditAction.Module,
                tableName: "AspNetUsers",
                recordId: request.Email,
                cancellationToken: cancellationToken);

            return result;
        }

        var action =
            result.Message.Contains(
                "locked",
                StringComparison.OrdinalIgnoreCase)
                ? SecurityAuditAction.UserLocked
                : SecurityAuditAction.LoginFailed;

        await _auditActionService.LogAsync(
            action: action,
            module: SecurityAuditAction.Module,
            tableName: "AspNetUsers",
            recordId: request.Email,
            newValues: new { result.Message },
            cancellationToken: cancellationToken);

        return result;
    }
}
