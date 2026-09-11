using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Common.Services;
using SearchEngine.Application.Features.PermissionActions.DTOs;
using SearchEngine.Domain.Entities;
using Mapster;
using MediatR;

namespace SearchEngine.Application.Features.PermissionActions.Commands.CreatePermissionAction;

public class CreatePermissionActionCommandHandler
    : IRequestHandler<
        CreatePermissionActionCommand,
        Result<PermissionActionResponse>>
{
    private readonly IApplicationIdentityDbContext _context;

    public CreatePermissionActionCommandHandler(
        IApplicationIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PermissionActionResponse>> Handle(
        CreatePermissionActionCommand request,
        CancellationToken cancellationToken)
    {
        var entity = request.Request
            .Adapt<PermissionAction>();

        await _context.PermissionActions.AddAsync(
            entity,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        // Rule: PermissionAction baru yang aktif -> generate Permission
        // untuk semua Module (Permission = generated catalog).
        if (entity.IsActive)
        {
            await PermissionCatalogSynchronizer.SyncAsync(
                _context,
                cancellationToken);
        }

        return Result<PermissionActionResponse>
            .SuccessResult(
                entity.Adapt<PermissionActionResponse>(),
                "Permission action created successfully");
    }
}
