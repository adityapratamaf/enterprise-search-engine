using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Auth.DTOs;
using MediatR;
using SearchEngine.Application.Common.Exceptions;

namespace SearchEngine.Application.Features.Auth.Queries.GetCurrentUser;

public class GetCurrentUserHandler
    : IRequestHandler<
        GetCurrentUserQuery,
        Result<CurrentUserAuthResponse>>
{
    private readonly ICurrentUserService
        _currentUserService;

    private readonly IIdentityService
        _identityService;

    public GetCurrentUserHandler(
        ICurrentUserService currentUserService,
        IIdentityService identityService)
    {
        _currentUserService = currentUserService;
        _identityService = identityService;
    }

    public async Task<Result<CurrentUserAuthResponse>>
        Handle(
            GetCurrentUserQuery request,
            CancellationToken cancellationToken)
    {
        var userId =
            _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedException("Unauthorized");
        }

        return await _identityService
            .GetCurrentUserAsync(userId);
    }
}
