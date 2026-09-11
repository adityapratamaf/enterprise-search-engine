using SearchEngine.Application.Common.Extensions;
using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Products.DTOs;
using SearchEngine.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Application.Features.Products.Queries.GetAllProducts;

public class GetAllProductsHandler : IRequestHandler<
        GetAllProductsQuery,
        Result<
            PaginatedResult<ReadProductResponse>>>
{
    private readonly IApplicationBusinessDbContext
        _context;

    public GetAllProductsHandler(
        IApplicationBusinessDbContext context)
    {
        _context = context;
    }

    public async Task<
        Result<
            PaginatedResult<ReadProductResponse>>>
        Handle(
            GetAllProductsQuery request,
            CancellationToken cancellationToken)
    {
        var query = _context.Products
            .AsNoTracking()
            .AsQueryable();

        // SEARCH
        if (!string.IsNullOrWhiteSpace(
            request.Request.Search))
        {
            query = query.Where(x =>
                x.Name.Contains(
                    request.Request.Search) ||

                x.Code.Contains(
                    request.Request.Search));
        }

        // SORT — always end with a stable Id tie-breaker so offset
        // pagination is deterministic: when the primary sort column has
        // duplicate values, page boundaries never skip or duplicate rows.
        var ordered = request.Request.SortBy?.ToLower() switch
        {
            "name" => request.Request.IsDescending
                ? query.OrderByDescending(
                    x => x.Name)
                : query.OrderBy(
                    x => x.Name),

            "price" => request.Request.IsDescending
                ? query.OrderByDescending(
                    x => x.Price)
                : query.OrderBy(
                    x => x.Price),

            _ => query.OrderBy(
                x => x.Name)
        };

        query = ordered.ThenBy(x => x.Id);

        var result = await query
            .ToPaginatedResultAsync<
                Product,
                ReadProductResponse>(
                    request.Request);

        return Result<
            PaginatedResult<ReadProductResponse>>
            .SuccessResult(result);
    }
}
