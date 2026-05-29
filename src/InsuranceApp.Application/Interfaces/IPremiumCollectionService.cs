using InsuranceApp.Contracts.PremiumCollections;

namespace InsuranceApp.Application.Interfaces;

public interface IPremiumCollectionService
{
    Task<PremiumCollectionItemResponse> CreateMandateAsync(CreatePremiumMandateRequest request, CancellationToken cancellationToken = default);
    Task<PremiumCollectionItemResponse> ScheduleCollectionAsync(string policyNumber, SchedulePremiumCollectionRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PremiumCollectionItemResponse>> ListPolicyCollectionsAsync(string policyNumber, CancellationToken cancellationToken = default);
    Task<PremiumCollectionRunResponse> RunDueCollectionsAsync(CancellationToken cancellationToken = default);
    Task<PremiumCollectionRunResponse> RetryFailedCollectionsAsync(CancellationToken cancellationToken = default);
}