using FluentValidation;

namespace SearchEngine.Application.Features.Roles.Commands.CreateRole;

public class CreateRoleValidator
    : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .OverridePropertyName("Name");
    }
}
