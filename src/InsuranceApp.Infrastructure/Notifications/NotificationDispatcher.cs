using InsuranceApp.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace InsuranceApp.Infrastructure.Notifications;

public sealed class NotificationDispatcher(
    IEnumerable<ISmsSender> smsSenders,
    IEnumerable<IEmailSender> emailSenders,
    IEnumerable<IWhatsAppSender> whatsAppSenders,
    ILogger<NotificationDispatcher> logger) : INotificationDispatcher
{
    private readonly IReadOnlyList<ISmsSender> _smsSenders = smsSenders.ToList();
    private readonly IReadOnlyList<IEmailSender> _emailSenders = emailSenders.ToList();
    private readonly IReadOnlyList<IWhatsAppSender> _whatsAppSenders = whatsAppSenders.ToList();

    public async Task<NotificationDeliveryResult> DispatchAsync(CustomerNotification notification, CancellationToken cancellationToken = default)
    {
        var preferred = notification.PreferredChannel?.Trim().ToUpperInvariant() ?? "SMS";
        var orderedChannels = preferred switch
        {
            "EMAIL" => new[] { "EMAIL", "SMS", "WHATSAPP" },
            "WHATSAPP" => new[] { "WHATSAPP", "SMS", "EMAIL" },
            _ => new[] { "SMS", "WHATSAPP", "EMAIL" }
        };

        foreach (var channel in orderedChannels)
        {
            var result = await TrySendAsync(channel, notification, cancellationToken);
            if (result.Status == NotificationDeliveryStatus.Sent)
            {
                return result;
            }

            logger.LogWarning("Channel {Channel} failed for {Reference}: {Message}", channel, notification.Reference, result.Message);
        }

        return new NotificationDeliveryResult(NotificationDeliveryStatus.Failed, notification.Reference, "All channels failed.");
    }

    private Task<NotificationDeliveryResult> TrySendAsync(string channel, CustomerNotification notification, CancellationToken cancellationToken)
    {
        switch (channel)
        {
            case "SMS":
                if (string.IsNullOrWhiteSpace(notification.Msisdn) || _smsSenders.Count == 0)
                {
                    return Task.FromResult(new NotificationDeliveryResult(NotificationDeliveryStatus.Skipped, notification.Reference, "No SMS destination."));
                }
                return _smsSenders[0].SendAsync(new SmsMessage(notification.Msisdn, notification.Body, notification.Reference), cancellationToken);
            case "EMAIL":
                if (string.IsNullOrWhiteSpace(notification.EmailAddress) || _emailSenders.Count == 0)
                {
                    return Task.FromResult(new NotificationDeliveryResult(NotificationDeliveryStatus.Skipped, notification.Reference, "No email destination."));
                }
                return _emailSenders[0].SendAsync(new EmailMessage(notification.EmailAddress, notification.Subject, notification.Body, notification.Reference), cancellationToken);
            case "WHATSAPP":
                if (string.IsNullOrWhiteSpace(notification.Msisdn) || _whatsAppSenders.Count == 0)
                {
                    return Task.FromResult(new NotificationDeliveryResult(NotificationDeliveryStatus.Skipped, notification.Reference, "No WhatsApp destination."));
                }
                return _whatsAppSenders[0].SendAsync(new WhatsAppMessage(notification.Msisdn, "policy_update", new[] { notification.Subject, notification.Body }, notification.Reference), cancellationToken);
            default:
                return Task.FromResult(new NotificationDeliveryResult(NotificationDeliveryStatus.Skipped, notification.Reference, $"Unknown channel {channel}"));
        }
    }
}
