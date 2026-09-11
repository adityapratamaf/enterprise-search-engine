using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;

using MediatR;

namespace SearchEngine.Application.Features.Users.Commands.ToggleUserStatus;

public class ToggleUserStatusHandler
    : IRequestHandler<
        ToggleUserStatusCommand,
        Result<string>>
{
    private readonly IIdentityService
        _identityService;

    public ToggleUserStatusHandler(
        IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<string>>
        Handle(
            ToggleUserStatusCommand request,
            CancellationToken cancellationToken)
    {
        return await _identityService
            .ToggleUserStatusAsync(
                request.Id);
    }
}
