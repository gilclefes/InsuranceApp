using System.Net.Http.Headers;
using System.Net.Http.Json;
using InsuranceApp.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InsuranceApp.Infrastructure.Payments.Providers;

public sealed class PaystackPaymentGateway(
    IHttpClientFactory httpClientFactory,
    IOptions<PaymentGatewayOptions> options,
    SimulatedPaymentGateway fallback,
    ILogger<PaystackPaymentGateway> logger) : IPaymentGateway
{
    public string ProviderCode => "PAYSTACK";

    public bool CanHandle(string providerCode) =>
        !string.IsNullOrWhiteSpace(providerCode) && providerCode.Equals(ProviderCode, StringComparison.OrdinalIgnoreCase);

    public async Task<PaymentGatewayChargeResponse> ChargeAsync(PaymentGatewayChargeRequest request, CancellationToken cancellationToken = default)
    {
        var cfg = options.Value.Paystack;
        if (!cfg.Enabled || string.IsNullOrWhiteSpace(cfg.ApiSecret))
        {
            return await fallback.ChargeAsync(request, cancellationToken);
        }

        try
        {
            var http = httpClientFactory.CreateClient(nameof(PaystackPaymentGateway));
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cfg.ApiSecret);
            var payload = new
            {
                amount = (long)(request.Amount * 100),
                email = request.CustomerEmail,
                currency = request.CurrencyCode,
                reference = request.IdempotencyKey
            };
            var response = await http.PostAsJsonAsync("/transaction/initialize", payload, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new PaymentGatewayChargeResponse(PaymentGatewayStatus.Failed, request.IdempotencyKey, $"Paystack HTTP {(int)response.StatusCode}", true);
            }

            return new PaymentGatewayChargeResponse(PaymentGatewayStatus.Pending, request.IdempotencyKey, "Paystack initialized; awaiting webhook.", false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Paystack charge failed for {Reference}", request.TransactionReference);
            return new PaymentGatewayChargeResponse(PaymentGatewayStatus.Failed, request.IdempotencyKey, ex.Message, true);
        }
    }
}
