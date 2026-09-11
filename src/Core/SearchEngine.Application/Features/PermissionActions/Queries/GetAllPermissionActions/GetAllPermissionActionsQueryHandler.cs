using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.PermissionActions.DTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Application.Features.PermissionActions.Queries.GetAllPermissionActions;

public class GetAllPermissionActionsQueryHandler
    : IRequestHandler<
        GetAllPermissionActionsQuery,
        Result<List<PermissionActionResponse>>>
{
    private readonly IApplicationIdentityDbContext _context;

    public GetAllPermissionActionsQueryHandler(
        IApplicationIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<PermissionActionResponse>>> Handle(
        GetAllPermissionActionsQuery request,
        CancellationToken cancellationToken)
    {
        var actions =
            await _context.PermissionActions
                .AsNoTracking()
                .OrderBy(x => x.DisplayOrder)
                .ProjectToType<PermissionActionResponse>()
                .ToListAsync(cancellationToken);

        return Result<List<PermissionActionResponse>>
            .SuccessResult(actions);
    }
}
