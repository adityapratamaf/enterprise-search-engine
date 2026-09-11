using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Users.DTOs;
using MediatR;

namespace SearchEngine.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdHandler
    : IRequestHandler<
        GetUserByIdQuery,
        Result<ReadUserResponse>>
{
    private readonly IIdentityService
        _identityService;

    public GetUserByIdHandler(
        IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<
        Result<ReadUserResponse>>
        Handle(
            GetUserByIdQuery request,
            CancellationToken cancellationToken)
    {
        return await _identityService
            .GetUserByIdAsync(
                request.Id);
    }
}
