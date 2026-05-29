using InsuranceApp.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace InsuranceApp.Infrastructure.Notifications;

public sealed class LoggingSmsSender(ILogger<LoggingSmsSender> logger) : ISmsSender
{
    public string Channel => "LOGGING";

    public Task<NotificationDeliveryResult> SendAsync(SmsMessage message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("SMS to {To} ref={Ref}: {Body}", message.ToMsisdn, message.Reference, message.Body);
        return Task.FromResult(new NotificationDeliveryResult(NotificationDeliveryStatus.Sent, $"LOG-{message.Reference}", "Logged."));
    }
}

public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public string Channel => "LOGGING";

    public Task<NotificationDeliveryResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("EMAIL to {To} ref={Ref} subject={Subject}", message.ToAddress, message.Reference, message.Subject);
        return Task.FromResult(new NotificationDeliveryResult(NotificationDeliveryStatus.Sent, $"LOG-{message.Reference}", "Logged."));
    }
}

public sealed class LoggingWhatsAppSender(ILogger<LoggingWhatsAppSender> logger) : IWhatsAppSender
{
    public string Channel => "LOGGING";

    public Task<NotificationDeliveryResult> SendAsync(WhatsAppMessage message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("WA to {To} ref={Ref} template={Template}", message.ToMsisdn, message.Reference, message.TemplateName);
        return Task.FromResult(new NotificationDeliveryResult(NotificationDeliveryStatus.Sent, $"LOG-{message.Reference}", "Logged."));
    }
}
