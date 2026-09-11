using SearchEngine.Application.Common.Models;
using MediatR;

namespace SearchEngine.Application.Features.FileAttachments.Queries.DownloadFile;

public record DownloadFileQuery(
    Guid Id)
    : IRequest<FileDownloadResult>;
