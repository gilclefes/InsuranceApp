using InsuranceApp.Application.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace InsuranceApp.Infrastructure.Otp;

public class HttpOtpSender(
    IHttpClientFactory httpClientFactory,
    ILogger<HttpOtpSender> logger,
    IOptions<OtpProviderOptions> providerOptions,
    LoggingOtpSender fallbackSender) : IOtpSender
{
    private readonly OtpProviderOptions _providerOptions = providerOptions.Value;

    public async Task SendAsync(string destination, string channel, string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_providerOptions.BaseUrl) || string.IsNullOrWhiteSpace(_providerOptions.ApiKey))
        {
            if (_providerOptions.UseLoggingFallback)
            {
                await fallbackSender.SendAsync(destination, channel, message, cancellationToken);
                return;
            }

            throw new InvalidOperationException("OTP provider is not configured. Set OtpProvider:BaseUrl and OtpProvider:ApiKey.");
        }

        try
        {
            var client = httpClientFactory.CreateClient(nameof(HttpOtpSender));
            var payload = new
            {
                to = destination,
                channel,
                message,
                senderId = _providerOptions.SenderId
            };

            using var response = await client.PostAsJsonAsync(_providerOptions.EndpointPath, payload, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError("OTP provider returned status {StatusCode}. Body: {Body}", (int)response.StatusCode, error);
                throw new InvalidOperationException("OTP provider request failed.");
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            logger.LogError(ex, "OTP provider send failed for destination {Destination}.", destination);
            if (_providerOptions.UseLoggingFallback)
            {
                await fallbackSender.SendAsync(destination, channel, message, cancellationToken);
                return;
            }

            throw;
        }
    }
}
