using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Products.DTOs;
using SearchEngine.Application.Common.Interfaces;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SearchEngine.Application.Common.Exceptions;

namespace SearchEngine.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdHandler : IRequestHandler<
        GetProductByIdQuery,
        Result<ReadProductResponse>>
{
    private readonly IApplicationBusinessDbContext _context;

    public GetProductByIdHandler(
        IApplicationBusinessDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ReadProductResponse>> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (product == null)
        {
            throw new NotFoundException("Product not found");
        }

        return Result<ReadProductResponse>
            .SuccessResult(product.Adapt<ReadProductResponse>());
    }
}
