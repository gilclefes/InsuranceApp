using System.Net.Http.Json;
using InsuranceApp.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InsuranceApp.Infrastructure.Payments.Providers;

public sealed class HubtelPaymentGateway(
    IHttpClientFactory httpClientFactory,
    IOptions<PaymentGatewayOptions> options,
    SimulatedPaymentGateway fallback,
    ILogger<HubtelPaymentGateway> logger) : IPaymentGateway
{
    public string ProviderCode => "HUBTEL";

    public bool CanHandle(string providerCode) =>
        !string.IsNullOrWhiteSpace(providerCode) && providerCode.StartsWith("HUBTEL", StringComparison.OrdinalIgnoreCase);

    public async Task<PaymentGatewayChargeResponse> ChargeAsync(PaymentGatewayChargeRequest request, CancellationToken cancellationToken = default)
    {
        var cfg = options.Value.Hubtel;
        if (!cfg.Enabled || string.IsNullOrWhiteSpace(cfg.BaseUrl))
        {
            return await fallback.ChargeAsync(request, cancellationToken);
        }

        try
        {
            var http = httpClientFactory.CreateClient(nameof(HubtelPaymentGateway));
            var payload = new
            {
                CustomerName = request.PolicyNumber,
                CustomerMsisdn = request.CustomerMsisdn,
                CustomerEmail = request.CustomerEmail,
                Channel = "mtn-gh",
                Amount = request.Amount,
                PrimaryCallbackUrl = $"{cfg.BaseUrl.TrimEnd('/')}/callback",
                Description = request.Description,
                ClientReference = request.IdempotencyKey
            };
            var response = await http.PostAsJsonAsync("/merchantaccount/merchants/" + cfg.MerchantAccountNumber + "/receive/mobilemoney", payload, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new PaymentGatewayChargeResponse(PaymentGatewayStatus.Failed, request.IdempotencyKey, $"Hubtel HTTP {(int)response.StatusCode}", true);
            }

            return new PaymentGatewayChargeResponse(PaymentGatewayStatus.Pending, request.IdempotencyKey, "Hubtel request accepted; awaiting webhook.", false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Hubtel charge failed for {Reference}", request.TransactionReference);
            return new PaymentGatewayChargeResponse(PaymentGatewayStatus.Failed, request.IdempotencyKey, ex.Message, true);
        }
    }
}
