using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SearchEngine.Application.Common.Exceptions;

namespace SearchEngine.Application.Features.Products.Commands.DeleteProduct;

public class DeleteProductHandler : IRequestHandler<
        DeleteProductCommand,
        Result<string>>
{
    private readonly IApplicationBusinessDbContext
        _context;

    private readonly IAuditActionService
        _auditActionService;

    private readonly IFileAttachmentCleaner
        _fileAttachmentCleaner;

    public DeleteProductHandler(
        IApplicationBusinessDbContext context,
        IAuditActionService auditActionService,
        IFileAttachmentCleaner fileAttachmentCleaner)
    {
        _context = context;
        _auditActionService =
            auditActionService;
        _fileAttachmentCleaner =
            fileAttachmentCleaner;
    }

    public async Task<
        Result<string>>
        Handle(
            DeleteProductCommand request,
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

        await _auditActionService.LogAsync(
            action: "DELETE",
            module: "Products",
            tableName: "Products",
            recordId:
                entity.Id.ToString(),
            oldValues:
                entity,
            cancellationToken:
                cancellationToken);

        _context.Products
            .Remove(entity);

        await _context.SaveChangesAsync(
            cancellationToken);

        // Cascade: bersihkan seluruh lampiran milik product ini
        // (baris database + file fisik) agar tidak menjadi orphan.
        await _fileAttachmentCleaner.CleanupByOwnerAsync(
            "products",
            entity.Id,
            cancellationToken);

        return Result<string>
            .SuccessResult(
                "Product deleted successfully");
    }
}
