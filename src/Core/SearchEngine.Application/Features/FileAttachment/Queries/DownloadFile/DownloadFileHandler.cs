using SearchEngine.Application.Common.Exceptions;
using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Application.Features.FileAttachments.Queries.DownloadFile;

public class DownloadFileHandler
    : IRequestHandler<
        DownloadFileQuery,
        FileDownloadResult>
{
    private readonly IApplicationBusinessDbContext
        _context;

    private readonly IPermissionAuthorizationService
        _permissions;

    public DownloadFileHandler(
        IApplicationBusinessDbContext context,
        IPermissionAuthorizationService permissions)
    {
        _context = context;
        _permissions = permissions;
    }

    public async Task<FileDownloadResult>
        Handle(
            DownloadFileQuery request,
            CancellationToken cancellationToken)
    {
        var file =
            await _context.FileAttachments
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == request.Id,
                    cancellationToken);

        if (file is null)
        {
            throw new NotFoundException(
                "File not found");
        }

        // Otorisasi mengikuti module pemilik file.
        if (!await _permissions.HasPermissionAsync(
                file.Module,
                "view"))
        {
            throw new ForbiddenException(
                "You do not have permission to download this file");
        }

        var rootPath =
            EnsureTrailingSeparator(
                Path.GetFullPath(
                    Directory.GetCurrentDirectory()));

        var fullPath =
            Path.GetFullPath(
                Path.Combine(
                    rootPath,
                    file.FilePath));

        if (!fullPath.StartsWith(
                rootPath,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new NotFoundException(
                "Physical file not found");
        }

        if (!File.Exists(fullPath))
        {
            throw new NotFoundException(
                "Physical file not found");
        }

        var stream =
            new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                options:
                    FileOptions.Asynchronous
                    | FileOptions.SequentialScan);

        return new FileDownloadResult
        {
            Content =
                stream,

            ContentType =
                file.ContentType,

            FileName =
                file.OriginalFileName
        };
    }

    private static string EnsureTrailingSeparator(
        string path)
    {
        return path.EndsWith(
            Path.DirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;
    }
}
