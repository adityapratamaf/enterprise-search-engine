using SearchEngine.Application.Features.Modules.Commands.UpdateModule;
using FluentValidation.TestHelper;

namespace SearchEngine.Application.UnitTests.Features.Modules;

public class UpdateModuleValidatorTests
{
    private readonly UpdateModuleValidator
        _validator = new();

    [Fact]
    public void Should_Have_Error_When_ModuleName_Is_Empty()
    {
        var command = new UpdateModuleCommand(
            Guid.NewGuid(),
            new()
            {
                ModuleId = "PRODUCT",
                ModuleName = "",
                ModulePath = "/products"
            });

        var result =
            _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("ModuleName");
    }

    [Fact]
    public void Should_Not_Have_Error()
    {
        var command = new UpdateModuleCommand(
            Guid.NewGuid(),
            new()
            {
                ModuleId = "PRODUCT",
                ModuleName = "Products",
                ModulePath = "/products"
            });

        var result =
            _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
