using SearchEngine.Application.Features.Auth.Commands.Login;
using FluentValidation.TestHelper;

namespace SearchEngine.Application.UnitTests.Features.Auth;

public class LoginValidatorTests
{
    private readonly LoginValidator _validator =
        new();

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var command = new LoginCommand
        {
            Email = "invalid-email",
            Password = "password"
        };

        var result =
            _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        var command = new LoginCommand
        {
            Email = "admin@searchengine.local",
            Password = ""
        };

        var result =
            _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            x => x.Password);
    }

    [Fact]
    public void Should_Not_Have_Error()
    {
        var command = new LoginCommand
        {
            Email = "admin@searchengine.local",
            Password = "password123"
        };

        var result =
            _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
