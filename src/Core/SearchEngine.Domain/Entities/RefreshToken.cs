using SearchEngine.Domain.Common;

namespace SearchEngine.Domain.Entities;

public class RefreshToken
    : BaseAuditableEntity
{
    public string UserId { get; set; }
        = default!;

    public string TokenHash { get; set; }
        = default!;

    public DateTime ExpiredAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string? CreatedByIp { get; set; }

    public string? RevokedByIp { get; set; }

    public string? UserAgent { get; set; }

    public bool IsRevoked { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public DateTime? LastUsedAt { get; set; }
}
