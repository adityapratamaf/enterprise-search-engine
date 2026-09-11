using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;

using MediatR;
using SearchEngine.Application.Common.Exceptions;

namespace SearchEngine.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordHandler
    : IRequestHandler<
        ChangePasswordCommand,
        Result<string>>
{
    private readonly IIdentityService
        _identityService;

    private readonly ICurrentUserService
        _currentUserService;

    private readonly IAuditActionService
        _auditActionService;

    public ChangePasswordHandler(
        IIdentityService identityService,
        ICurrentUserService currentUserService,
        IAuditActionService auditActionService)
    {
        _identityService = identityService;
        _currentUserService = currentUserService;
        _auditActionService = auditActionService;
    }

    public async Task<Result<string>>
        Handle(
            ChangePasswordCommand request,
            CancellationToken cancellationToken)
    {
        // Current user selalu diambil dari identity yang terautentikasi,
        // tidak pernah dari request.
        var userId =
            _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedException("Unauthorized");
        }

        var result = await _identityService
            .ChangePasswordAsync(
                userId,
                request.Request);

        if (!result.Success)
        {
            return result;
        }

        // Hanya mencatat bahwa password berhasil diubah.
        // Tidak pernah mencatat current/new password maupun hash.
        await _auditActionService.LogAsync(
            action: "CHANGE_PASSWORD",
            module: "Profile",
            tableName: "AspNetUsers",
            recordId: userId,
            cancellationToken:
                cancellationToken);

        return result;
    }
}
