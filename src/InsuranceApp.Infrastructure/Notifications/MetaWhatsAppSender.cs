using System.Net.Http.Headers;
using System.Net.Http.Json;
using InsuranceApp.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InsuranceApp.Infrastructure.Notifications;

public sealed class MetaWhatsAppSender(
    IHttpClientFactory httpClientFactory,
    IOptions<NotificationOptions> options,
    LoggingWhatsAppSender fallback,
    ILogger<MetaWhatsAppSender> logger) : IWhatsAppSender
{
    public string Channel => "META";

    public async Task<NotificationDeliveryResult> SendAsync(WhatsAppMessage message, CancellationToken cancellationToken = default)
    {
        var cfg = options.Value.WhatsApp;
        if (!cfg.Enabled || string.IsNullOrWhiteSpace(cfg.AccessToken))
        {
            return await fallback.SendAsync(message, cancellationToken);
        }

        try
        {
            var http = httpClientFactory.CreateClient(nameof(MetaWhatsAppSender));
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cfg.AccessToken);
            var payload = new
            {
                messaging_product = "whatsapp",
                to = message.ToMsisdn,
                type = "template",
                template = new
                {
                    name = message.TemplateName,
                    language = new { code = "en" },
                    components = new[]
                    {
                        new
                        {
                            type = "body",
                            parameters = message.Parameters.Select(p => new { type = "text", text = p }).ToArray()
                        }
                    }
                }
            };
            var url = $"{cfg.BaseUrl.TrimEnd('/')}/{cfg.PhoneNumberId}/messages";
            var response = await http.PostAsJsonAsync(url, payload, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new NotificationDeliveryResult(NotificationDeliveryStatus.Failed, message.Reference, $"WhatsApp HTTP {(int)response.StatusCode}");
            }
            return new NotificationDeliveryResult(NotificationDeliveryStatus.Sent, message.Reference, "WhatsApp accepted.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "WhatsApp send failed");
            return new NotificationDeliveryResult(NotificationDeliveryStatus.Failed, message.Reference, ex.Message);
        }
    }
}
