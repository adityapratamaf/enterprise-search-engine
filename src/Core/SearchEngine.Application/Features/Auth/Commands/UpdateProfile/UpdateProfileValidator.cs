using FluentValidation;

namespace SearchEngine.Application.Features.Auth.Commands.UpdateProfile;

public class UpdateProfileValidator
    : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.Request.Username)
            .NotEmpty()
            .OverridePropertyName("Username");

        RuleFor(x => x.Request.Email)
            .NotEmpty()
            .EmailAddress()
            .OverridePropertyName("Email");

        RuleFor(x => x.Request.FirstName)
            .NotEmpty()
            .OverridePropertyName("FirstName");

        RuleFor(x => x.Request.LastName)
            .NotEmpty()
            .OverridePropertyName("LastName");

        // Password bersifat opsional. Bila kosong, password tidak diubah.
        // Validasi kekuatan password ditangani oleh ASP.NET Identity
        // saat proses perubahan password.
    }
}
