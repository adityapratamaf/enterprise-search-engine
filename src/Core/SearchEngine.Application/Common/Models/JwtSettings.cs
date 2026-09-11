using System.ComponentModel.DataAnnotations;

namespace SearchEngine.Application.Common.Models;

public class JwtSettings
{
    public const string SectionName =
        "JwtSettings";

    [Required]
    [MinLength(32)]
    public string Secret { get; set; } = default!;

    [Required]
    public string Issuer { get; set; } = default!;

    [Required]
    public string Audience { get; set; } = default!;

    [Range(1, 1440)]
    public int AccessTokenExpirationMinutes { get; set; }

    [Range(1, 365)]
    public int RefreshTokenExpirationDays { get; set; }
}
