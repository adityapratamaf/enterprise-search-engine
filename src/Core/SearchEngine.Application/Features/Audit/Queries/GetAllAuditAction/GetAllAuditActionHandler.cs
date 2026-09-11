using SearchEngine.Application.Common.Extensions;
using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Audit.DTOs;
using System.Text.Json;
using SearchEngine.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Application.Features.Audit.Queries.GetAllAuditAction;

public class GetAllAuditActionHandler
    : IRequestHandler<
        GetAllAuditActionQuery,
        Result<
            PaginatedResult<ReadAuditActionResponse>>>
{
    private readonly IApplicationIdentityDbContext
        _context;

    public GetAllAuditActionHandler(
        IApplicationIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<
        Result<
            PaginatedResult<ReadAuditActionResponse>>>
        Handle(
            GetAllAuditActionQuery request,
            CancellationToken cancellationToken)
    {
        var query = _context.AuditLogs
            .AsNoTracking()
            .AsQueryable();

        // SEARCH
        if (!string.IsNullOrWhiteSpace(
            request.Request.Search))
        {
            query = query.Where(x =>

                x.Action.Contains(
                    request.Request.Search) ||

                x.Module.Contains(
                    request.Request.Search) ||

                x.TableName.Contains(
                    request.Request.Search) ||

                x.RecordId.Contains(
                    request.Request.Search));
        }

        // SORT — always end with a stable Id tie-breaker so offset
        // pagination is deterministic. This matters most for the default
        // CreatedAt ordering, where duplicate timestamps would otherwise let
        // page boundaries skip or duplicate rows.
        var ordered = request.Request
            .SortBy?.ToLower() switch
        {
            "action" =>
                request.Request.IsDescending
                    ? query.OrderByDescending(
                        x => x.Action)
                    : query.OrderBy(
                        x => x.Action),

            "module" =>
                request.Request.IsDescending
                    ? query.OrderByDescending(
                        x => x.Module)
                    : query.OrderBy(
                        x => x.Module),

            _ =>
                request.Request.IsDescending
                    ? query.OrderByDescending(
                        x => x.CreatedAt)
                    : query.OrderBy(
                        x => x.CreatedAt)
        };

        query = ordered.ThenBy(x => x.Id);

        var result = await query
            .ToPaginatedResultAsync<
                AuditAction,
                ReadAuditActionResponse>(
                    request.Request);

        foreach (var item in result.Items)
        {
            item.OldValues =
                string.IsNullOrWhiteSpace(
                    item.OldValues?.ToString())
                    ? null
                    : JsonSerializer.Deserialize<object>(
                        item.OldValues.ToString()!);

            item.NewValues =
                string.IsNullOrWhiteSpace(
                    item.NewValues?.ToString())
                    ? null
                    : JsonSerializer.Deserialize<object>(
                        item.NewValues.ToString()!);
        }

        return Result<
            PaginatedResult<ReadAuditActionResponse>>
            .SuccessResult(result);
    }
}
