using SearchEngine.Application.Common.Exceptions;
using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Application.Features.FileAttachments.Commands.DeleteFile;

public class DeleteFileHandler
    : IRequestHandler<
        DeleteFileCommand,
        Result<string>>
{
    private readonly IApplicationBusinessDbContext
        _context;

    private readonly IFileAttachmentService
        _storage;

    private readonly IAuditActionService
        _auditLogService;

    private readonly IPermissionAuthorizationService
        _permissions;

    public DeleteFileHandler(
        IApplicationBusinessDbContext context,
        IFileAttachmentService storage,
        IAuditActionService auditLogService,
        IPermissionAuthorizationService permissions)
    {
        _context = context;
        _storage = storage;
        _auditLogService = auditLogService;
        _permissions = permissions;
    }

    public async Task<Result<string>>
        Handle(
            DeleteFileCommand request,
            CancellationToken cancellationToken)
    {
        var entity =
            await _context.FileAttachments
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
                "delete"))
        {
            throw new ForbiddenException(
                "You do not have permission to delete this file");
        }

        // Delete File Fisik
        await _storage.DeleteAsync(
            entity.FilePath,
            cancellationToken);

        // Audit Action
        await _auditLogService.LogAsync(
            action: "DELETE",
            module: "FileAttachments",
            tableName: "FileAttachments",
            recordId: entity.Id.ToString(),
            oldValues: entity,
            cancellationToken:
                cancellationToken);

        _context.FileAttachments
            .Remove(entity);

        await _context.SaveChangesAsync(
            cancellationToken);

        return Result<string>
            .SuccessResult(
                "Success",
                "File deleted successfully");
    }
}
