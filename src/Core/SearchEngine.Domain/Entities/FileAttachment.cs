using SearchEngine.Domain.Common;

namespace SearchEngine.Domain.Entities;

public class FileAttachment
    : BaseAuditableEntity
{
    // Generic Reference

    public string Module { get; set; } = null!;

    public Guid RecordId { get; set; }

    // File Info

    public string FileName { get; set; } = null!;

    public string OriginalFileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long FileSize { get; set; }

    public string FilePath { get; set; } = null!;

    public string StorageProvider { get; set; } = null!;
}
