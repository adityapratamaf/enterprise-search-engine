using SearchEngine.Application.Common.Exceptions;
using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.FileAttachments.DTOs;
using SearchEngine.Domain.Entities;
using Mapster;
using MediatR;

namespace SearchEngine.Application.Features.FileAttachments.Commands.UploadFile;

public class UploadFileHandler
    : IRequestHandler<
        UploadFileCommand,
        Result<
            List<ReadFileAttachmentResponse>>>
{
    private readonly IApplicationBusinessDbContext
        _context;

    private readonly IFileAttachmentService
        _storage;

    private readonly IPermissionAuthorizationService
        _permissions;

    public UploadFileHandler(
        IApplicationBusinessDbContext context,
        IFileAttachmentService storage,
        IPermissionAuthorizationService permissions)
    {
        _context = context;
        _storage = storage;
        _permissions = permissions;
    }

    public async Task<
        Result<
            List<ReadFileAttachmentResponse>>>
        Handle(
            UploadFileCommand request,
            CancellationToken cancellationToken)
    {
        // Otorisasi mengikuti module pemilik: butuh izin create pada module itu.
        if (!await _permissions.HasPermissionAsync(
                request.Module,
                "create"))
        {
            throw new ForbiddenException(
                "You do not have permission to upload files for this module");
        }

        var entities =
            new List<FileAttachment>();

        foreach (var file in request.Files)
        {
            var uploadResult =
                await _storage.UploadAsync(
                    file.FileStream,
                    file.FileName,
                    file.ContentType,
                    cancellationToken);

            var entity =
                new FileAttachment
                {
                    Module =
                        request.Module,

                    RecordId =
                        request.RecordId,

                    FileName =
                        uploadResult.FileName,

                    OriginalFileName =
                        file.FileName,

                    ContentType =
                        file.ContentType,

                    FileSize =
                        uploadResult.FileSize,

                    FilePath =
                        uploadResult.FilePath,

                    StorageProvider =
                        uploadResult.StorageProvider
                };

            await _context.FileAttachments
                .AddAsync(
                    entity,
                    cancellationToken);

            entities.Add(entity);
        }

        await _context.SaveChangesAsync(
            cancellationToken);

        var result =
            entities.Adapt<
                List<ReadFileAttachmentResponse>>();

        return Result<
            List<ReadFileAttachmentResponse>>
            .SuccessResult(
                result,
                "Files uploaded successfully");
    }
}
