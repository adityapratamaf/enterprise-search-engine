using SearchEngine.Domain.Common;

namespace SearchEngine.Domain.Entities;

/// <summary>
/// Durable record used to make email background jobs idempotent under
/// Hangfire's at-least-once execution. The entity <see cref="BaseEntity.Id"/>
/// doubles as the idempotency key for a single logical email operation.
/// </summary>
public class EmailOutbox
    : BaseEntity
{
    public string Recipient { get; set; } = default!;

    public string Subject { get; set; } = default!;

    public string Status { get; set; } =
        EmailOutboxStatus.Pending;

    public int Attempts { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? SentAt { get; set; }

    public string? Error { get; set; }
}

public static class EmailOutboxStatus
{
    public const string Pending = "Pending";
    public const string Sending = "Sending";
    public const string Sent = "Sent";
    public const string Failed = "Failed";
}
