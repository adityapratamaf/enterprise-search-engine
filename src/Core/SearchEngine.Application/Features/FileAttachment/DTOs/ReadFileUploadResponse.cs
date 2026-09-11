namespace SearchEngine.Application.Features.FileAttachments.DTOs;

public class ReadFileUploadResponse
{
    public string FileName { get; set; } = null!;
    public string FilePath { get; set; } = null!;
    public long FileSize { get; set; }
    public string StorageProvider { get; set; } = null!;
}
