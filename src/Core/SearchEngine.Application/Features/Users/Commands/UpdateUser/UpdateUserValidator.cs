using FluentValidation;

namespace SearchEngine.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserValidator
    : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Request.Username)
            .NotEmpty()
            .OverridePropertyName("Username");

        RuleFor(x => x.Request.Email)
            .NotEmpty()
            .EmailAddress()
            .OverridePropertyName("Email");

        RuleFor(x => x.Request.Role)
            .NotEmpty()
            .OverridePropertyName("Role");
    }
}
