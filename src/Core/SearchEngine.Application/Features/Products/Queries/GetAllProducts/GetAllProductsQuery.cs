using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Products.DTOs;
using MediatR;

namespace SearchEngine.Application.Features.Products.Queries.GetAllProducts;

public record GetAllProductsQuery(
    PaginationRequest Request)
    : IRequest<
        Result<
            PaginatedResult<ReadProductResponse>>>;
