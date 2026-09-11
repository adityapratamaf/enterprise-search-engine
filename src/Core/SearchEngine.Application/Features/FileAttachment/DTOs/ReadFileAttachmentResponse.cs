namespace SearchEngine.Application.Features.FileAttachments.DTOs;

public class ReadFileAttachmentResponse
{
    public Guid Id { get; set; }
    public string Module { get; set; } = null!;
    public Guid RecordId { get; set; }
    public string FileName { get; set; } = null!;
    public string OriginalFileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long FileSize { get; set; }
    public string FilePath { get; set; } = null!;
    public string StorageProvider { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
