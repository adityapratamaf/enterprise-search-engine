using FluentValidation;

namespace SearchEngine.Application.Features.Modules.Commands.UpdateModule;

public class UpdateModuleValidator
    : AbstractValidator<UpdateModuleCommand>
{
    public UpdateModuleValidator()
    {
        RuleFor(x => x.Request.ModuleId)
            .NotEmpty()
            .MaximumLength(100)
            .OverridePropertyName("ModuleId");

        RuleFor(x => x.Request.ModuleName)
            .NotEmpty()
            .MaximumLength(150)
            .OverridePropertyName("ModuleName");

        RuleFor(x => x.Request.ModulePath)
            .NotEmpty()
            .MaximumLength(250)
            .OverridePropertyName("ModulePath");
    }
}
