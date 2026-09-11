using FluentValidation;

namespace SearchEngine.Application.Features.FileAttachments.Queries.GetAllFiles;

public class GetAllFilesValidator
    : AbstractValidator<
        GetAllFilesQuery>
{
    public GetAllFilesValidator()
    {
        RuleFor(x => x.Module)
            .NotEmpty()
            .WithMessage(
                "Module is required");

        RuleFor(x => x.RecordId)
            .NotEmpty()
            .WithMessage(
                "RecordId is required");
    }
}
