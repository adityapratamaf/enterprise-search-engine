using FluentValidation;

namespace SearchEngine.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenValidator
    : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}
