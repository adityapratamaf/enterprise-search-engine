using SearchEngine.Application.Features.Products.Commands.CreateProduct;
using SearchEngine.Application.Features.Products.DTOs;
using FluentValidation.TestHelper;

namespace SearchEngine.Application.UnitTests.Features.Products;

public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator =
        new();

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command =
            new CreateProductCommand(
                new CreateProductRequest
                {
                    Name = "",
                    Code = "PRD001",
                    Price = 100,
                    Stock = 1
                });

        var result =
            _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Name");
    }

    [Fact]
    public void Should_Have_Error_When_Code_Is_Empty()
    {
        var command =
            new CreateProductCommand(
                new CreateProductRequest
                {
                    Name = "Laptop",
                    Code = "",
                    Price = 100,
                    Stock = 1
                });

        var result =
            _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Code");
    }

    [Fact]
    public void Should_Have_Error_When_Price_Is_Zero()
    {
        var command =
            new CreateProductCommand(
                new CreateProductRequest
                {
                    Name = "Laptop",
                    Code = "PRD001",
                    Price = 0,
                    Stock = 1
                });

        var result =
            _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Price");
    }

    [Fact]
    public void Should_Not_Have_Validation_Error()
    {
        var command =
            new CreateProductCommand(
                new CreateProductRequest
                {
                    Name = "Laptop",
                    Code = "PRD001",
                    Price = 100,
                    Stock = 1
                });

        var result =
            _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
