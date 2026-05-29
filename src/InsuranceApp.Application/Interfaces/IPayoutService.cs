using InsuranceApp.Contracts.Payouts;

namespace InsuranceApp.Application.Interfaces;

public interface IPayoutService
{
    Task<PayoutResponse> InitiatePayoutAsync(InitiatePayoutRequest request, CancellationToken cancellationToken = default);
    Task<PayoutResponse> GetPayoutAsync(string payoutReference, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PayoutResponse>> ListPayoutsAsync(string? status, string? claimNumber, DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default);
    Task<PayoutReconciliationRunResponse> RunReconciliationAsync(CancellationToken cancellationToken = default);
    Task<PayoutWebhookResponse> ProcessWebhookAsync(PayoutWebhookRequest request, CancellationToken cancellationToken = default);
}