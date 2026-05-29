using InsuranceApp.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InsuranceApp.Infrastructure.Payments.Providers;

/// GhIPSS ACH is a file-based settlement scheme; we record an "Initiated" state pending T+1 settlement file ingestion.
public sealed class GhIpssAchPaymentGateway(
    IOptions<PaymentGatewayOptions> options,
    SimulatedPaymentGateway fallback,
    ILogger<GhIpssAchPaymentGateway> logger) : IPaymentGateway
{
    public string ProviderCode => "GHIPSS_ACH";

    public bool CanHandle(string providerCode) =>
        !string.IsNullOrWhiteSpace(providerCode) &&
        (providerCode.Equals(ProviderCode, StringComparison.OrdinalIgnoreCase) ||
         providerCode.Equals("GHIPSS_GIP", StringComparison.OrdinalIgnoreCase));

    public Task<PaymentGatewayChargeResponse> ChargeAsync(PaymentGatewayChargeRequest request, CancellationToken cancellationToken = default)
    {
        var cfg = options.Value.GhIpss;
        if (!cfg.Enabled)
        {
            return fallback.ChargeAsync(request, cancellationToken);
        }

        logger.LogInformation("Queued GhIPSS ACH debit for {Reference} amount {Amount}", request.TransactionReference, request.Amount);
        return Task.FromResult(new PaymentGatewayChargeResponse(
            PaymentGatewayStatus.Pending,
            $"ACH-{request.IdempotencyKey}",
            "Queued in ACH batch awaiting settlement file.",
            false));
    }
}
