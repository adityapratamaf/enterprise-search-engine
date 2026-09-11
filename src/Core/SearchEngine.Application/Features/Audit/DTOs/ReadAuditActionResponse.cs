namespace SearchEngine.Application.Features.Audit.DTOs;

public class ReadAuditActionResponse
{
    public Guid Id { get; set; }
    public string? UserEmail { get; set; }
    public string Action { get; set; } = default!;
    public string Module { get; set; } = default!;
    public string TableName { get; set; } = default!;
    public string RecordId { get; set; } = default!;
    public object? OldValues { get; set; }
    public object? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
}
