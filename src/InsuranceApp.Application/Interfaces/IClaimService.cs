using InsuranceApp.Contracts.Claims;

namespace InsuranceApp.Application.Interfaces;

public interface IClaimService
{
    Task<ClaimResponse> CreateClaimAsync(CreateClaimRequest request, CancellationToken cancellationToken = default);
    Task<ClaimResponse> GetClaimAsync(string claimNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ClaimResponse>> ListClaimsAsync(string? status, string? assignedAdjusterId, string? policyNumber, CancellationToken cancellationToken = default);
    Task<ClaimResponse> AssignClaimAsync(string claimNumber, AssignClaimRequest request, CancellationToken cancellationToken = default);
    Task<ClaimResponse> ReviewClaimAsync(string claimNumber, ReviewClaimRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ClaimTimelineEventResponse>> GetTimelineAsync(string claimNumber, CancellationToken cancellationToken = default);
    Task<ClaimSlaDashboardResponse> GetSlaDashboardAsync(string? assignedAdjusterId, CancellationToken cancellationToken = default);
}