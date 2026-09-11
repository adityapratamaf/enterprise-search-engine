using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Modules.DTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Application.Features.Modules.Queries.GetAllModules;

public class GetAllModulesQueryHandler
    : IRequestHandler<
        GetAllModulesQuery,
        Result<List<ReadModuleResponse>>>
{
    private readonly IApplicationIdentityDbContext _context;

    public GetAllModulesQueryHandler(
        IApplicationIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ReadModuleResponse>>> Handle(
        GetAllModulesQuery request,
        CancellationToken cancellationToken)
    {
        var modules = await _context.Modules
            .AsNoTracking()
            .ProjectToType<ReadModuleResponse>()
            .ToListAsync(cancellationToken);

        return Result<List<ReadModuleResponse>>
            .SuccessResult(modules);
    }
}
