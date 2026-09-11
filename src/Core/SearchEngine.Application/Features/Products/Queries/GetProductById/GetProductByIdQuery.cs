using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Products.DTOs;
using MediatR;

namespace SearchEngine.Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id)
    : IRequest<Result<ReadProductResponse>>;
