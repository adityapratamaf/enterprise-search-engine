namespace SearchEngine.Application.Common.Models;

public sealed class EmailMessage
{
    public string To { get; set; } = default!;

    public string Subject { get; set; } = default!;

    public string Body { get; set; } = default!;
}
