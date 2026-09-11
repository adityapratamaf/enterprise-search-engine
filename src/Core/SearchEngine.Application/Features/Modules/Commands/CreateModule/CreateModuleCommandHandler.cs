using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Common.Services;
using SearchEngine.Application.Features.Modules.DTOs;
using SearchEngine.Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SearchEngine.Application.Common.Exceptions;

namespace SearchEngine.Application.Features.Modules.Commands.CreateModule;

public class CreateModuleCommandHandler
    : IRequestHandler<
        CreateModuleCommand,
        Result<ReadModuleResponse>>
{
    private readonly IApplicationIdentityDbContext _context;

    public CreateModuleCommandHandler(
        IApplicationIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ReadModuleResponse>> Handle(
        CreateModuleCommand request,
        CancellationToken cancellationToken)
    {
        var exists =
            await _context.Modules
                .AnyAsync(
                    x => x.ModuleId ==
                         request.Request.ModuleId,
                    cancellationToken);

        if (exists)
        {
            throw new ConflictException("Module already exists");
        }

        var entity = request.Request
            .Adapt<Module>();

        await _context.Modules.AddAsync(
            entity,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);

        // Rule: Module baru -> generate Permission untuk semua
        // PermissionAction yang aktif (Permission = generated catalog).
        await PermissionCatalogSynchronizer.SyncAsync(
            _context,
            cancellationToken);

        var result = entity
            .Adapt<ReadModuleResponse>();

        return Result<ReadModuleResponse>
            .SuccessResult(
                result,
                "Module created successfully");
    }
}
