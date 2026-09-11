using System.ComponentModel.DataAnnotations;

namespace SearchEngine.Infrastructure.Shared.Email;

public sealed class SmtpSettings
{
    public const string SectionName =
        "SMTP";

    [Required]
    public string Host { get; set; } = default!;

    [Range(1, 65535)]
    public int Port { get; set; }

    [Required]
    public string Username { get; set; } = default!;

    [Required]
    public string Password { get; set; } = default!;

    [Required]
    [EmailAddress]
    public string FromEmail { get; set; } = default!;

    [Required]
    public string FromName { get; set; } = default!;
}
