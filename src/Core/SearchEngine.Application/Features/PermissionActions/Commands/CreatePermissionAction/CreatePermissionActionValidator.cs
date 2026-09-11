using SearchEngine.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Application.Features.PermissionActions.Commands.CreatePermissionAction;

public class CreatePermissionActionValidator
    : AbstractValidator<CreatePermissionActionCommand>
{
    private readonly IApplicationIdentityDbContext _context;

    public CreatePermissionActionValidator(
        IApplicationIdentityDbContext context)
    {
        _context = context;

        RuleFor(x => x.Request.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MustAsync(BeUniqueCode)
            .WithMessage("Code already exists")
            .OverridePropertyName("Code");

        RuleFor(x => x.Request.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MustAsync(BeUniqueName)
            .WithMessage("Name already exists")
            .OverridePropertyName("Name");

        RuleFor(x => x.Request.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .OverridePropertyName("DisplayOrder");
    }

    private async Task<bool> BeUniqueCode(
        string? code,
        CancellationToken cancellationToken)
    {
        return !await _context.PermissionActions
            .AnyAsync(
                x => x.Code == code,
                cancellationToken);
    }

    private async Task<bool> BeUniqueName(
        string? name,
        CancellationToken cancellationToken)
    {
        var normalized = (name ?? string.Empty).ToLower();

        return !await _context.PermissionActions
            .AnyAsync(
                x => x.Name.ToLower() == normalized,
                cancellationToken);
    }
}
