using SearchEngine.Application.Features.Auth.Commands.RefreshToken;
using FluentValidation.TestHelper;

namespace SearchEngine.Application.UnitTests.Features.Auth;

public class RefreshTokenValidatorTests
{
    private readonly RefreshTokenValidator
        _validator = new();

    [Fact]
    public void Should_Have_Error_When_RefreshToken_Is_Empty()
    {
        var command =
            new RefreshTokenCommand
            {
                RefreshToken = ""
            };

        var result =
            _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            x => x.RefreshToken);
    }

    [Fact]
    public void Should_Not_Have_Error()
    {
        var command =
            new RefreshTokenCommand
            {
                RefreshToken =
                    "abc123token"
            };

        var result =
            _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
