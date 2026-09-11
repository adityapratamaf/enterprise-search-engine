using SearchEngine.Application.Common.Security;
using SearchEngine.Application.Features.FileAttachments.DTOs;
using FluentValidation;

namespace SearchEngine.Application.Features.FileAttachments.Commands.UploadFile;

public class UploadFileValidator
    : AbstractValidator<
        UploadFileCommand>
{
    private const long
        MaxFileSize =
            20 * 1024 * 1024;

    public UploadFileValidator()
    {
        RuleFor(x => x.Files)
            .NotEmpty()
            .WithMessage(
                "At least one file is required");

        RuleForEach(x => x.Files)
            .Must(HaveValidExtension)
            .WithMessage(
                "File type is not allowed");

        RuleForEach(x => x.Files)
            .Must(x =>
                x.FileStream.Length <=
                MaxFileSize)
            .WithMessage(
                "Maximum file size is 20 MB");

        RuleForEach(x => x.Files)
            .Must(HaveValidSignature)
            .WithMessage(
                "File content does not match its extension");
    }

    private static bool HaveValidExtension(
        CreateFileUploadRequest file)
    {
        return FileSignatureValidator.IsAllowedExtension(
            file.FileName);
    }

    private static bool HaveValidSignature(
        CreateFileUploadRequest file)
    {
        return FileSignatureValidator.HasValidSignature(
            file.FileStream,
            file.FileName);
    }
}
