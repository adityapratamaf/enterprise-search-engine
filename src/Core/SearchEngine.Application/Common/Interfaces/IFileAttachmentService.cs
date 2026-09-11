using SearchEngine.Application.Features.FileAttachments.DTOs;

namespace SearchEngine.Application.Common.Interfaces;

public interface IFileAttachmentService
{
    Task<ReadFileUploadResponse>
        UploadAsync(
            Stream stream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken);

    Task DeleteAsync(
        string filePath,
        CancellationToken cancellationToken);
}
