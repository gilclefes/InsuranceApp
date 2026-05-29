using System.Net.Http.Headers;
using System.Text;
using InsuranceApp.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InsuranceApp.Infrastructure.Notifications;

public sealed class HubtelSmsSender(
    IHttpClientFactory httpClientFactory,
    IOptions<NotificationOptions> options,
    LoggingSmsSender fallback,
    ILogger<HubtelSmsSender> logger) : ISmsSender
{
    public string Channel => "HUBTEL";

    public async Task<NotificationDeliveryResult> SendAsync(SmsMessage message, CancellationToken cancellationToken = default)
    {
        var cfg = options.Value.Hubtel;
        if (!cfg.Enabled || string.IsNullOrWhiteSpace(cfg.ClientId))
        {
            return await fallback.SendAsync(message, cancellationToken);
        }

        try
        {
            var http = httpClientFactory.CreateClient(nameof(HubtelSmsSender));
            var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{cfg.ClientId}:{cfg.ClientSecret}"));
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basic);
            var url = $"{cfg.BaseUrl.TrimEnd('/')}/v1/messages/send?clientid={cfg.ClientId}&from={Uri.EscapeDataString(cfg.From)}&to={Uri.EscapeDataString(message.ToMsisdn)}&content={Uri.EscapeDataString(message.Body)}";
            var response = await http.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new NotificationDeliveryResult(NotificationDeliveryStatus.Failed, message.Reference, $"Hubtel HTTP {(int)response.StatusCode}");
            }
            return new NotificationDeliveryResult(NotificationDeliveryStatus.Sent, message.Reference, "Hubtel accepted.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Hubtel SMS failed");
            return new NotificationDeliveryResult(NotificationDeliveryStatus.Failed, message.Reference, ex.Message);
        }
    }
}
