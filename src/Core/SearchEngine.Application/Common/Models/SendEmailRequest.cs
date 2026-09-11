namespace SearchEngine.Application.Common.Models;

public sealed class SendEmailRequest
{
    public string Email { get; set; } = default!;

    public string Subject { get; set; } = default!;

    public string Message { get; set; } = default!;
}
