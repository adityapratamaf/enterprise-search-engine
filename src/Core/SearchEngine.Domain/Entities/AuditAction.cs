namespace SearchEngine.Domain.Entities;

public class AuditAction
{
    public Guid Id { get; set; }

    public string? UserEmail { get; set; }

    public string Action { get; set; } = default!;

    public string Module { get; set; } = default!;

    public string TableName { get; set; } = default!;

    public string RecordId { get; set; } = default!;

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
