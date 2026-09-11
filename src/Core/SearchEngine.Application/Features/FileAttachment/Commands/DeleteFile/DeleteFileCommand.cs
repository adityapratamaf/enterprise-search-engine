using SearchEngine.Application.Common.Models;
using MediatR;

namespace SearchEngine.Application.Features.FileAttachments.Commands.DeleteFile;

public record DeleteFileCommand(
    Guid Id)
    : IRequest<Result<string>>;
