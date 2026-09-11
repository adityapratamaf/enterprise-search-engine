using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Products.DTOs;
using MediatR;

namespace SearchEngine.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(Guid Id, UpdateProductRequest Request) : IRequest<
        Result<ReadProductResponse>>;
