using FluentValidation;

namespace SearchEngine.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordValidator
    : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.Request.CurrentPassword)
            .NotEmpty()
            .OverridePropertyName("CurrentPassword");

        RuleFor(x => x.Request.NewPassword)
            .NotEmpty()
            .OverridePropertyName("NewPassword");

        RuleFor(x => x.Request.ConfirmPassword)
            .NotEmpty()
            .Equal(x => x.Request.NewPassword)
            .WithMessage("Password confirmation does not match.")
            .OverridePropertyName("ConfirmPassword");

        // Kekuatan password baru divalidasi oleh ASP.NET Identity
        // (password policy) saat proses perubahan password.
    }
}
