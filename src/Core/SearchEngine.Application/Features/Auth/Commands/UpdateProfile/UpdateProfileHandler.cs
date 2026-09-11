using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Users.DTOs;

using MediatR;
using SearchEngine.Application.Common.Exceptions;

namespace SearchEngine.Application.Features.Auth.Commands.UpdateProfile;

public class UpdateProfileHandler
    : IRequestHandler<
        UpdateProfileCommand,
        Result<ReadUserResponse>>
{
    private readonly IIdentityService
        _identityService;

    private readonly ICurrentUserService
        _currentUserService;

    private readonly IAuditActionService
        _auditActionService;

    public UpdateProfileHandler(
        IIdentityService identityService,
        ICurrentUserService currentUserService,
        IAuditActionService auditActionService)
    {
        _identityService = identityService;
        _currentUserService = currentUserService;
        _auditActionService = auditActionService;
    }

    public async Task<Result<ReadUserResponse>>
        Handle(
            UpdateProfileCommand request,
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

        // OLD VALUES
        var oldUser = await _identityService
            .GetUserByIdAsync(userId);

        var result = await _identityService
            .UpdateProfileAsync(
                userId,
                request.Request);

        if (!result.Success || result.Data is null)
        {
            return result;
        }

        await _auditActionService.LogAsync(
            action: "UPDATE",
            module: "Profile",
            tableName: "AspNetUsers",
            recordId: userId,
            oldValues: oldUser.Data,
            newValues: result.Data,
            cancellationToken:
                cancellationToken);

        return result;
    }
}
