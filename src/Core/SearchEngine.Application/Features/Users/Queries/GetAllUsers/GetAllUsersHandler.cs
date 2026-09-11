using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Users.DTOs;

using MediatR;

namespace SearchEngine.Application.Features.Users.Queries.GetAllUsers;

public class GetAllUsersHandler
    : IRequestHandler<
        GetAllUsersQuery,
        Result<
            PaginatedResult<ReadUserResponse>>>
{
    private readonly IIdentityService
        _identityService;

    public GetAllUsersHandler(
        IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<
        Result<
            PaginatedResult<ReadUserResponse>>>
        Handle(
            GetAllUsersQuery request,
            CancellationToken cancellationToken)
    {
        return await _identityService
            .GetAllUsersAsync(
                request.Request);
    }
}
