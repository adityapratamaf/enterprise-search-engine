using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.FileAttachments.DTOs;
using MediatR;

namespace SearchEngine.Application.Features.FileAttachments.Queries.GetFileById;

public record GetFileByIdQuery(
    Guid Id)
    : IRequest<
        Result<ReadFileAttachmentResponse>>;
