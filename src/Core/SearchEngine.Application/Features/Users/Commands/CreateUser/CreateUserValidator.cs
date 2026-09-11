using FluentValidation;

namespace SearchEngine.Application.Features.Users.Commands.CreateUser;

public class CreateUserValidator
    : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Request.Username)
            .NotEmpty()
            .OverridePropertyName("Username");

        RuleFor(x => x.Request.Email)
            .NotEmpty()
            .EmailAddress()
            .OverridePropertyName("Email");

        RuleFor(x => x.Request.Password)
            .NotEmpty()
            .MinimumLength(6)
            .OverridePropertyName("Password");

        RuleFor(x => x.Request.Role)
            .NotEmpty()
            .OverridePropertyName("Role");
    }
}
