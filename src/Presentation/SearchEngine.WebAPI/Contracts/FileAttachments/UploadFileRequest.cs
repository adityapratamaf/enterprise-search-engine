namespace SearchEngine.WebAPI.Contracts.FileAttachments;

public class UploadFileRequest
{
    public string Module { get; set; } = null!;

    public Guid RecordId { get; set; }

    public List<IFormFile> Files { get; set; } = [];
}
