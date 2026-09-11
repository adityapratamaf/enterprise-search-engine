using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Products.DTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SearchEngine.Application.Common.Exceptions;

namespace SearchEngine.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand,
        Result<ReadProductResponse>>
{
    private readonly IApplicationBusinessDbContext
        _context;

    private readonly IAuditActionService
        _auditActionService;

    public UpdateProductHandler(
        IApplicationBusinessDbContext context,
        IAuditActionService auditActionService)
    {
        _context = context;
        _auditActionService =
            auditActionService;
    }

    public async Task<
        Result<ReadProductResponse>>
        Handle(
            UpdateProductCommand request,
            CancellationToken cancellationToken)
    {
        var entity =
            await _context.Products
                .FirstOrDefaultAsync(
                    x => x.Id ==
                         request.Id,
                    cancellationToken);

        if (entity is null)
        {
            throw new NotFoundException("Product not found");
        }

        var oldValues =
            entity.Adapt<ReadProductResponse>();

        entity.Name =
            request.Request.Name;

        entity.Code =
            request.Request.Code;

        entity.Price =
            request.Request.Price;

        entity.Stock =
            request.Request.Stock;

        entity.Description =
            request.Request.Description;

        await _context.SaveChangesAsync(
            cancellationToken);

        var dto =
            entity.Adapt<ReadProductResponse>();

        await _auditActionService.LogAsync(
            action: "UPDATE",
            module: "Products",
            tableName: "Products",
            recordId:
                entity.Id.ToString(),
            oldValues: oldValues,
            newValues: dto,
            cancellationToken:
                cancellationToken);

        return Result<ReadProductResponse>
            .SuccessResult(
                dto,
                "Product updated successfully");
    }
}
