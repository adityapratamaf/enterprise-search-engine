namespace SearchEngine.Application.Common.Models;

public class FileDownloadResult
{
    public Stream Content { get; set; } = Stream.Null;

    public string ContentType { get; set; } = null!;

    public string FileName { get; set; } = null!;
}
