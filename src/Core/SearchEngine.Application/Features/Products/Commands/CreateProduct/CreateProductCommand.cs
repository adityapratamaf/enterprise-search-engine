using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Products.DTOs;
using MediatR;

namespace SearchEngine.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(CreateProductRequest Request) : IRequest<
    Result<ReadProductResponse>>;
