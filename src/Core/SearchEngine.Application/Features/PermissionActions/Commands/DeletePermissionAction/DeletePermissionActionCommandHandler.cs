using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SearchEngine.Application.Common.Exceptions;

namespace SearchEngine.Application.Features.PermissionActions.Commands.DeletePermissionAction;

public class DeletePermissionActionCommandHandler
    : IRequestHandler<
        DeletePermissionActionCommand,
        Result<bool>>
{
    private readonly IApplicationIdentityDbContext _context;

    public DeletePermissionActionCommandHandler(
        IApplicationIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(
        DeletePermissionActionCommand request,
        CancellationToken cancellationToken)
    {
        var entity =
            await _context.PermissionActions
                .FirstOrDefaultAsync(
                    x => x.Id == request.Id,
                    cancellationToken);

        if (entity is null)
        {
            throw new NotFoundException("Permission action not found");
        }

        // Rule: hard delete hanya diperbolehkan bila action belum pernah
        // menghasilkan Permission (belum menjadi bagian katalog).
        var isPartOfCatalog =
            await _context.Permissions
                .AnyAsync(
                    p => p.PermissionActionId == request.Id,
                    cancellationToken);

        if (isPartOfCatalog)
        {
            return Result<bool>
                .Failure(
                    "Permission action is already part of the permission catalog and cannot be deleted. Set IsActive = false to retire it instead.");
        }

        _context.PermissionActions.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>
            .SuccessResult(
                true,
                "Permission action deleted successfully");
    }
}
