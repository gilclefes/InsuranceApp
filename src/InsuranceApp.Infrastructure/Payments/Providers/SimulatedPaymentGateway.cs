using InsuranceApp.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace InsuranceApp.Infrastructure.Payments.Providers;

public sealed class SimulatedPaymentGateway(ILogger<SimulatedPaymentGateway> logger) : IPaymentGateway
{
    public string ProviderCode => "SIMULATED";

    public bool CanHandle(string providerCode) =>
        string.IsNullOrWhiteSpace(providerCode) ||
        providerCode.Equals(ProviderCode, StringComparison.OrdinalIgnoreCase) ||
        providerCode.Equals("MANUAL", StringComparison.OrdinalIgnoreCase);

    public Task<PaymentGatewayChargeResponse> ChargeAsync(PaymentGatewayChargeRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            return Task.FromResult(new PaymentGatewayChargeResponse(
                PaymentGatewayStatus.Failed,
                string.Empty,
                "Amount must be greater than zero.",
                false));
        }

        // Deterministic simulation: odd integral amounts fail once.
        var simulateFailure = (long)Math.Round(request.Amount) % 2 != 0;
        if (simulateFailure)
        {
            logger.LogInformation("Simulated payment failure for tx {Reference}", request.TransactionReference);
            return Task.FromResult(new PaymentGatewayChargeResponse(
                PaymentGatewayStatus.Failed,
                $"SIM-{request.IdempotencyKey}",
                "Simulated provider timeout.",
                true));
        }

        return Task.FromResult(new PaymentGatewayChargeResponse(
            PaymentGatewayStatus.Success,
            $"SIM-{request.IdempotencyKey}",
            "Simulated charge succeeded.",
            false));
    }
}
