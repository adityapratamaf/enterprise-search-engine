using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.FileAttachments.DTOs;
using MediatR;

namespace SearchEngine.Application.Features.FileAttachments.Queries.GetAllFiles;

public class GetAllFilesQuery
    : PaginationRequest,
      IRequest<
        Result<
            PaginatedResult<
                ReadFileAttachmentResponse>>>
{
    public string Module { get; set; } = null!;

    public Guid RecordId { get; set; }
}
