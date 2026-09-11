using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.FileAttachments.DTOs;

using MediatR;

namespace SearchEngine.Application.Features.FileAttachments.Commands.UploadFile;

public record UploadFileCommand(
    string Module,
    Guid RecordId,
    List<CreateFileUploadRequest> Files)
    : IRequest<
        Result<
            List<ReadFileAttachmentResponse>>>;
