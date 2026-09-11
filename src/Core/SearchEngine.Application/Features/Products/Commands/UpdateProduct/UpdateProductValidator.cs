using SearchEngine.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductValidator : AbstractValidator<
        UpdateProductCommand>
{
    private readonly IApplicationBusinessDbContext
        _context;

    public UpdateProductValidator(
        IApplicationBusinessDbContext context)
    {
        _context = context;

        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Request.Code)
            .NotEmpty()
            .MaximumLength(50)
            .MustAsync(
                BeUniqueCode)
            .WithMessage(
                "Product code already exists");

        RuleFor(x => x.Request.Price)
            .GreaterThan(0);

        RuleFor(x => x.Request.Stock)
            .GreaterThanOrEqualTo(0);
    }

    private async Task<bool>
        BeUniqueCode(
            UpdateProductCommand command,
            string code,
            CancellationToken cancellationToken)
    {
        return !await _context.Products
            .AnyAsync(
                x =>
                    x.Code == code
                    &&
                    x.Id != command.Id,
                cancellationToken);
    }
}
