using InsuranceApp.Contracts.Privacy;

namespace InsuranceApp.Application.Interfaces;

public interface IDataRetentionService
{
    Task<DataRetentionSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<DataRetentionRunResponse> RunRetentionAsync(CancellationToken cancellationToken = default);
}