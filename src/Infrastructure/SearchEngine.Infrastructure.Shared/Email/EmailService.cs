using SearchEngine.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;

namespace SearchEngine.Infrastructure.Shared.Email;

public sealed class EmailService : IEmailService
{
    private readonly SmtpSettings _smtp;

    public EmailService(
        IOptions<SmtpSettings> smtp)
    {
        _smtp = smtp.Value;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody)
    {
        var email = new MimeMessage();

        email.From.Add(
            new MailboxAddress(
                _smtp.FromName,
                _smtp.FromEmail));

        email.To.Add(
            MailboxAddress.Parse(to));

        email.Subject = subject;

        email.Body =
            new TextPart("html")
            {
                Text = htmlBody
            };

        using var smtp =
            new SmtpClient();

        await smtp.ConnectAsync(
            _smtp.Host,
            _smtp.Port,
            SecureSocketOptions.StartTls);

        await smtp.AuthenticateAsync(
            _smtp.Username,
            _smtp.Password);

        await smtp.SendAsync(email);

        await smtp.DisconnectAsync(true);
    }
}
