using FluentValidation;

namespace SearchEngine.Application.Features.Products.Commands.CreateProduct;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(200)
            .WithName("Name")
            .OverridePropertyName("Name");

        RuleFor(x => x.Request.Code)
            .NotEmpty()
            .MaximumLength(50)
            .WithName("Code")
            .OverridePropertyName("Code");

        RuleFor(x => x.Request.Price)
            .NotNull()
            .GreaterThan(0)
            .WithName("Price")
            .OverridePropertyName("Price");

        RuleFor(x => x.Request.Stock)
            .NotNull()
            .GreaterThanOrEqualTo(0)
            .WithName("Stock")
            .OverridePropertyName("Stock");
    }
}
