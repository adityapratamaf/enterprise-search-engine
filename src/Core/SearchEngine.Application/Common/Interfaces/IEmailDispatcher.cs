namespace SearchEngine.Application.Common.Interfaces;

/// <summary>
/// Sends an email idempotently. Safe to invoke multiple times (Hangfire
/// retries / concurrent workers) for the same <paramref name="id"/>: the
/// email is delivered at most once per successful claim.
/// </summary>
public interface IEmailDispatcher
{
    Task DispatchAsync(
        Guid id,
        string to,
        string subject,
        string htmlBody);
}
