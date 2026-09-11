using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Infrastructure.Shared.Email;

/// <summary>
/// Idempotent email dispatcher backed by the EmailOutbox table.
///
/// Delivery guarantee: at-most-once per successful claim. A crash between
/// claiming a row ("Sending") and the provider accepting the message can leave
/// an email unsent (row stuck in "Sending"); this is the deliberate trade-off
/// to avoid duplicate sends. Failures before/at send transition the row to
/// "Failed" so a Hangfire retry can re-claim and resend it.
/// </summary>
public sealed class EmailDispatcher
    : IEmailDispatcher
{
    private readonly IApplicationBusinessDbContext _context;

    private readonly IEmailService _emailService;

    public EmailDispatcher(
        IApplicationBusinessDbContext context,
        IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task DispatchAsync(
        Guid id,
        string to,
        string subject,
        string htmlBody)
    {
        var now = DateTime.UtcNow;

        // Atomic claim: a single UPDATE flips exactly one Pending/Failed row to
        // Sending. Concurrent workers or Hangfire retries cannot both claim it.
        var claimed =
            await _context.EmailOutboxes
                .Where(x =>
                    x.Id == id
                    && (x.Status == EmailOutboxStatus.Pending
                        || x.Status == EmailOutboxStatus.Failed))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Status, EmailOutboxStatus.Sending)
                    .SetProperty(x => x.Attempts, x => x.Attempts + 1));

        if (claimed == 0)
        {
            // Already Sent, already being sent, or unknown id -> idempotent skip.
            return;
        }

        try
        {
            await _emailService.SendAsync(to, subject, htmlBody);

            await _context.EmailOutboxes
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Status, EmailOutboxStatus.Sent)
                    .SetProperty(x => x.SentAt, now)
                    .SetProperty(x => x.Error, (string?)null));
        }
        catch (Exception ex)
        {
            var error =
                ex.Message.Length > 500
                    ? ex.Message[..500]
                    : ex.Message;

            // Mark Failed so a retry can re-claim, then rethrow for Hangfire.
            await _context.EmailOutboxes
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Status, EmailOutboxStatus.Failed)
                    .SetProperty(x => x.Error, error));

            throw;
        }
    }
}
