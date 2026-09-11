using SearchEngine.Application.Common.Exceptions;
using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.FileAttachments.DTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Application.Features.FileAttachments.Queries.GetFileById;

public class GetFileByIdHandler
    : IRequestHandler<
        GetFileByIdQuery,
        Result<ReadFileAttachmentResponse>>
{
    private readonly IApplicationBusinessDbContext
        _context;

    private readonly IPermissionAuthorizationService
        _permissions;

    public GetFileByIdHandler(
        IApplicationBusinessDbContext context,
        IPermissionAuthorizationService permissions)
    {
        _context = context;
        _permissions = permissions;
    }

    public async Task<
        Result<ReadFileAttachmentResponse>>
        Handle(
            GetFileByIdQuery request,
            CancellationToken cancellationToken)
    {
        var entity =
            await _context.FileAttachments
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == request.Id,
                    cancellationToken);

        if (entity is null)
        {
            throw new NotFoundException(
                "File not found");
        }

        // Otorisasi mengikuti module pemilik file.
        if (!await _permissions.HasPermissionAsync(
                entity.Module,
                "view"))
        {
            throw new ForbiddenException(
                "You do not have permission to view this file");
        }

        return Result<ReadFileAttachmentResponse>
            .SuccessResult(
                entity.Adapt<
                    ReadFileAttachmentResponse>());
    }
}
