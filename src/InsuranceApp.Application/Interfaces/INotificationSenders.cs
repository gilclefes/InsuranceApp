namespace InsuranceApp.Application.Interfaces;

public enum NotificationDeliveryStatus
{
    Sent,
    Failed,
    Skipped
}

public sealed record NotificationDeliveryResult(NotificationDeliveryStatus Status, string ProviderReference, string Message);

public sealed record SmsMessage(string ToMsisdn, string Body, string Reference);
public sealed record EmailMessage(string ToAddress, string Subject, string HtmlBody, string Reference);
public sealed record WhatsAppMessage(string ToMsisdn, string TemplateName, IReadOnlyList<string> Parameters, string Reference);

public interface ISmsSender
{
    string Channel { get; }
    Task<NotificationDeliveryResult> SendAsync(SmsMessage message, CancellationToken cancellationToken = default);
}

public interface IEmailSender
{
    string Channel { get; }
    Task<NotificationDeliveryResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}

public interface IWhatsAppSender
{
    string Channel { get; }
    Task<NotificationDeliveryResult> SendAsync(WhatsAppMessage message, CancellationToken cancellationToken = default);
}

public sealed record CustomerNotification(
    string PreferredChannel,
    string Msisdn,
    string EmailAddress,
    string Subject,
    string Body,
    string Reference);

public interface INotificationDispatcher
{
    Task<NotificationDeliveryResult> DispatchAsync(CustomerNotification notification, CancellationToken cancellationToken = default);
}
