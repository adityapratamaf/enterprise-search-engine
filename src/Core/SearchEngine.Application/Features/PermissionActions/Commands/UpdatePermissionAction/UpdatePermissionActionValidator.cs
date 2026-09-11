using SearchEngine.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Application.Features.PermissionActions.Commands.UpdatePermissionAction;

public class UpdatePermissionActionValidator
    : AbstractValidator<UpdatePermissionActionCommand>
{
    private readonly IApplicationIdentityDbContext _context;

    public UpdatePermissionActionValidator(
        IApplicationIdentityDbContext context)
    {
        _context = context;

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

    // Name unik (case-insensitive) mengabaikan record dirinya sendiri.
    private async Task<bool> BeUniqueName(
        UpdatePermissionActionCommand command,
        string? name,
        CancellationToken cancellationToken)
    {
        var normalized = (name ?? string.Empty).ToLower();

        return !await _context.PermissionActions
            .AnyAsync(
                x =>
                    x.Id != command.Id &&
                    x.Name.ToLower() == normalized,
                cancellationToken);
    }
}
