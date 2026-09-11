using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Products.DTOs;
using SearchEngine.Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SearchEngine.Application.Common.Exceptions;

namespace SearchEngine.Application.Features.Products.Commands.CreateProduct;

public class CreateProductHandler : IRequestHandler<
        CreateProductCommand,
        Result<ReadProductResponse>>
{
    private readonly IApplicationBusinessDbContext _context;

    private readonly IAuditActionService
        _auditActionService;

    public CreateProductHandler(
        IApplicationBusinessDbContext context,
        IAuditActionService auditLogService)
    {
        _context = context;
        _auditActionService = auditLogService;
    }

    public async Task<Result<ReadProductResponse>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var exists =
            await _context.Products
                .AnyAsync(
                    x => x.Code ==
                         request.Request.Code,
                    cancellationToken);

        if (exists)
        {
            throw new ConflictException("Product code already exists");
        }

        var entity = request.Request
            .Adapt<Product>();

        await _context.Products.AddAsync(
            entity,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);

        await _auditActionService.LogAsync(
            action: "CREATE",
            module: "Products",
            tableName: "Products",
            recordId: entity.Id.ToString(),
            newValues: entity);

        var result = entity
            .Adapt<ReadProductResponse>();

        return Result<ReadProductResponse>
            .SuccessResult(result);
    }
}
