using SearchEngine.Application.Common.Security;
using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Domain.Entities;
using SearchEngine.Infrastructure.Shared.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Hangfire;

namespace SearchEngine.WebAPI.Controllers;

[ApiController]
[Authorize]
[EnableRateLimiting("api")]
[Route("api/email")]
public class EmailController : ControllerBase
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    private readonly IApplicationBusinessDbContext _context;

    public EmailController(
        IBackgroundJobClient backgroundJobClient,
        IApplicationBusinessDbContext context)
    {
        _backgroundJobClient = backgroundJobClient;
        _context = context;
    }

    [HttpPost("send")]
    [HasPermission("email", "view")]
    public async Task<IActionResult> SendEmail(
        [FromBody] SendEmailRequest request)
    {
        var html =
            EmailTemplateBuilder.BuildTemplate(
                "EmailTemplate",
                new Dictionary<string, string>
                {
                    ["Title"] = request.Subject,
                    ["Message"] = request.Message
                });

        // Durable outbox record; its Id is the idempotency key so Hangfire
        // retries deliver the email at most once.
        var emailId = Guid.NewGuid();

        _context.EmailOutboxes.Add(
            new EmailOutbox
            {
                Id = emailId,
                Recipient = request.Email,
                Subject = request.Subject,
                Status = EmailOutboxStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });

        await _context.SaveChangesAsync(
            HttpContext.RequestAborted);

        var jobId =
            _backgroundJobClient.Enqueue<IEmailDispatcher>(
                dispatcher => dispatcher.DispatchAsync(
                    emailId,
                    request.Email,
                    request.Subject,
                    html));

        return Ok(Result<object>.SuccessResult(
            new
            {
                jobId,
                emailId,
                email = request.Email,
                subject = request.Subject
            },
            "Email queued successfully"));
    }
}
