using InsuranceApp.Contracts.Policies;

namespace InsuranceApp.Application.Interfaces;

public interface IPolicyIssuanceService
{
    Task<IssuePolicyFromQuoteResponse> IssueFromQuoteAsync(IssuePolicyFromQuoteRequest request, CancellationToken cancellationToken = default);
    Task<PolicyDocumentResponse> GetPolicyDocumentAsync(string policyNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PolicySummaryResponse>> GetDashboardAsync(long? customerId, string? agentUserId, string? status, CancellationToken cancellationToken = default);
    Task<PolicyOperationResponse> EndorsePolicyAsync(string policyNumber, EndorsePolicyRequest request, CancellationToken cancellationToken = default);
    Task<PolicyOperationResponse> CancelPolicyAsync(string policyNumber, CancelPolicyRequest request, CancellationToken cancellationToken = default);
    Task<RenewalReminderRunResponse> RunRenewalReminderCycleAsync(CancellationToken cancellationToken = default);
}
