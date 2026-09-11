namespace SearchEngine.Application.Features.FileAttachments.DTOs;

public class CreateFileUploadRequest
{
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public Stream FileStream { get; set; } = null!;
}
