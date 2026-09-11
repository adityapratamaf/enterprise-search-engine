using SearchEngine.Application.Common.Exceptions;
using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.FileAttachments.DTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Application.Features.FileAttachments.Queries.GetAllFiles;

public class GetAllFilesHandler
    : IRequestHandler<
        GetAllFilesQuery,
        Result<
            PaginatedResult<
                ReadFileAttachmentResponse>>>
{
    private const int MaxPageSize = 100;

    private readonly IApplicationBusinessDbContext
        _context;

    private readonly IPermissionAuthorizationService
        _permissions;

    public GetAllFilesHandler(
        IApplicationBusinessDbContext context,
        IPermissionAuthorizationService permissions)
    {
        _context = context;
        _permissions = permissions;
    }

    public async Task<
        Result<
            PaginatedResult<
                ReadFileAttachmentResponse>>>
        Handle(
            GetAllFilesQuery request,
            CancellationToken cancellationToken)
    {
        // Otorisasi mengikuti module pemilik: butuh izin view pada module itu.
        if (!await _permissions.HasPermissionAsync(
                request.Module,
                "view"))
        {
            throw new ForbiddenException(
                "You do not have permission to view files for this module");
        }

        var query =
            _context.FileAttachments
                .AsNoTracking()
                .Where(x =>
                    x.Module ==
                    request.Module

                    &&

                    x.RecordId ==
                    request.RecordId);

        if (!string.IsNullOrWhiteSpace(
                request.Search))
        {
            query =
                query.Where(x =>
                    x.FileName.Contains(
                        request.Search)

                    ||

                    x.OriginalFileName.Contains(
                        request.Search));
        }

        var totalCount =
            await query.CountAsync(
                cancellationToken);

        var pageNumber =
            Math.Max(1, request.PageNumber);

        var pageSize =
            Math.Clamp(
                request.PageSize,
                1,
                MaxPageSize);

        var items =
            await query
                .Skip(
                    (pageNumber - 1)
                    * pageSize)

                .Take(
                    pageSize)

                .ProjectToType<
                    ReadFileAttachmentResponse>()

                .ToListAsync(
                    cancellationToken);

        var result =
            new PaginatedResult<
                ReadFileAttachmentResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber =
                    pageNumber,
                PageSize =
                    pageSize
            };

        return Result<
            PaginatedResult<
                ReadFileAttachmentResponse>>
            .SuccessResult(
                result);
    }
}
