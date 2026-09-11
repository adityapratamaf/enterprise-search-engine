using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SearchEngine.Application.Common.Exceptions;

namespace SearchEngine.Application.Features.Modules.Commands.DeleteModule;

public class DeleteModuleCommandHandler
    : IRequestHandler<
        DeleteModuleCommand,
        Result<bool>>
{
    private readonly IApplicationIdentityDbContext
        _context;

    public DeleteModuleCommandHandler(
        IApplicationIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(
        DeleteModuleCommand request,
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

        _context.Modules.Remove(module);

        await _context.SaveChangesAsync(
            cancellationToken);

        return Result<bool>
            .SuccessResult(
                true,
                "Module deleted successfully");
    }
}
