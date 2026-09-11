using SearchEngine.Application.Common.Models;
using MediatR;

namespace SearchEngine.Application.Features.Products.Commands.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest<
        Result<string>>;
