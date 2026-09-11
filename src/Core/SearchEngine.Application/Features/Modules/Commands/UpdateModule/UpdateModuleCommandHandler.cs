using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SearchEngine.Application.Common.Exceptions;

namespace SearchEngine.Application.Features.Modules.Commands.UpdateModule;

public class UpdateModuleCommandHandler
    : IRequestHandler<
        UpdateModuleCommand,
        Result<bool>>
{
    private readonly IApplicationIdentityDbContext
        _context;

    public UpdateModuleCommandHandler(
        IApplicationIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(
        UpdateModuleCommand request,
        CancellationToken cancellationToken)
    {
        var module =
    await _context.Modules
        .FirstOrDefaultAsync(
            x => x.Id == request.Id,
            cancellationToken);

        if (module is null)
        {
            throw new NotFoundException("Module not found");
        }

        var exists =
            await _context.Modules
                .AnyAsync(
                    x =>
                        x.ModuleId ==
                        request.Request.ModuleId
                        &&
                        x.Id != request.Id,
                    cancellationToken);

        if (exists)
        {
            throw new ConflictException("ModuleId already exists");
        }

        module.ModuleId =
            request.Request.ModuleId;

        module.ModuleName =
            request.Request.ModuleName;

        module.ModulePath =
            request.Request.ModulePath;

        await _context.SaveChangesAsync(
            cancellationToken);

        return Result<bool>
            .SuccessResult(
                true,
                "Module updated successfully");
    }
}
