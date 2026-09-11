using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Common.Services;
using SearchEngine.Application.Features.PermissionActions.DTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SearchEngine.Application.Common.Exceptions;

namespace SearchEngine.Application.Features.PermissionActions.Commands.UpdatePermissionAction;

public class UpdatePermissionActionCommandHandler
    : IRequestHandler<
        UpdatePermissionActionCommand,
        Result<PermissionActionResponse>>
{
    private readonly IApplicationIdentityDbContext _context;

    public UpdatePermissionActionCommandHandler(
        IApplicationIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PermissionActionResponse>> Handle(
        UpdatePermissionActionCommand request,
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

        // Code immutable: hanya metadata yang boleh diubah.
        entity.Name = request.Request.Name;
        entity.Description = request.Request.Description;
        entity.DisplayOrder = request.Request.DisplayOrder;
        entity.IsActive = request.Request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        // Menjaga invariant katalog: action yang (kini) aktif harus punya
        // Permission untuk semua Module. Menonaktifkan bersifat non-destruktif
        // (katalog & grant lama tetap ada).
        if (entity.IsActive)
        {
            await PermissionCatalogSynchronizer.SyncAsync(
                _context,
                cancellationToken);
        }

        return Result<PermissionActionResponse>
            .SuccessResult(
                entity.Adapt<PermissionActionResponse>(),
                "Permission action updated successfully");
    }
}
