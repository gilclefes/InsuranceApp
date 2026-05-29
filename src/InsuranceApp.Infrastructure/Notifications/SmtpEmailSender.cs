using System.Net;
using System.Net.Mail;
using InsuranceApp.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InsuranceApp.Infrastructure.Notifications;

public sealed class SmtpEmailSender(
    IOptions<NotificationOptions> options,
    LoggingEmailSender fallback,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public string Channel => "SMTP";

    public async Task<NotificationDeliveryResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var cfg = options.Value.Smtp;
        if (!cfg.Enabled || string.IsNullOrWhiteSpace(cfg.Host))
        {
            return await fallback.SendAsync(message, cancellationToken);
        }

        try
        {
            using var client = new SmtpClient(cfg.Host, cfg.Port)
            {
                EnableSsl = cfg.EnableSsl,
                Credentials = string.IsNullOrWhiteSpace(cfg.Username) ? CredentialCache.DefaultNetworkCredentials : new NetworkCredential(cfg.Username, cfg.Password)
            };
            using var mail = new MailMessage
            {
                From = new MailAddress(cfg.FromAddress, cfg.FromName),
                Subject = message.Subject,
                Body = message.HtmlBody,
                IsBodyHtml = true
            };
            mail.To.Add(message.ToAddress);
            await client.SendMailAsync(mail, cancellationToken);
            return new NotificationDeliveryResult(NotificationDeliveryStatus.Sent, message.Reference, "SMTP accepted.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "SMTP send failed");
            return new NotificationDeliveryResult(NotificationDeliveryStatus.Failed, message.Reference, ex.Message);
        }
    }
}
