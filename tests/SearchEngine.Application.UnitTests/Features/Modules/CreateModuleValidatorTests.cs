using SearchEngine.Application.Features.Modules.Commands.CreateModule;
using FluentValidation.TestHelper;

namespace SearchEngine.Application.UnitTests.Features.Modules;

public class CreateModuleValidatorTests
{
    private readonly CreateModuleCommandValidator
        _validator = new();

    [Fact]
    public void Should_Have_Error_When_ModuleId_Is_Empty()
    {
        var command = new CreateModuleCommand(
            new()
            {
                ModuleId = "",
                ModuleName = "Products",
                ModulePath = "/products"
            });

        var result =
            _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("ModuleId");
    }

    [Fact]
    public void Should_Not_Have_Error()
    {
        var command = new CreateModuleCommand(
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
