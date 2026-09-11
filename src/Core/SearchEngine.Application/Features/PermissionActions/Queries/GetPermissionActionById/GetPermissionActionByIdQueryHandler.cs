using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.PermissionActions.DTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SearchEngine.Application.Common.Exceptions;

namespace SearchEngine.Application.Features.PermissionActions.Queries.GetPermissionActionById;

public class GetPermissionActionByIdQueryHandler
    : IRequestHandler<
        GetPermissionActionByIdQuery,
        Result<PermissionActionResponse>>
{
    private readonly IApplicationIdentityDbContext _context;

    public GetPermissionActionByIdQueryHandler(
        IApplicationIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PermissionActionResponse>> Handle(
        GetPermissionActionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var action =
            await _context.PermissionActions
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == request.Id,
                    cancellationToken);

        if (action is null)
        {
            throw new NotFoundException("Permission action not found");
        }

        return Result<PermissionActionResponse>
            .SuccessResult(
                action.Adapt<PermissionActionResponse>());
    }
}
