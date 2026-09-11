using SearchEngine.Application.Common.Models;

using Mapster;

using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Application.Common.Extensions;

public static class PaginationExtensions
{
    private const int MaxPageSize = 100;

    public static async Task<
        PaginatedResult<TDestination>>
        ToPaginatedResultAsync<
            TSource,
            TDestination>(
            this IQueryable<TSource> query,
            PaginationRequest request)
    {
        var pageNumber =
            Math.Max(1, request.PageNumber);

        var pageSize =
            Math.Clamp(
                request.PageSize,
                1,
                MaxPageSize);

        var totalCount = await query
            .CountAsync();

        var items = await query

            .Skip(
                (pageNumber - 1)
                * pageSize)

            .Take(pageSize)

            .ProjectToType<TDestination>()

            .ToListAsync();

        return new PaginatedResult<TDestination>
        {
            Items = items,

            TotalCount = totalCount,

            PageNumber = pageNumber,

            PageSize = pageSize
        };
    }
}
