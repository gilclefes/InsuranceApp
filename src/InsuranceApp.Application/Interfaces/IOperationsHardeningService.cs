using InsuranceApp.Contracts.Operations;

namespace InsuranceApp.Application.Interfaces;

public interface IOperationsHardeningService
{
    Task<OperationsReadinessSummaryResponse> GetReadinessSummaryAsync(CancellationToken cancellationToken = default);
    Task<FailoverDrillRunResponse> RunFailoverDrillAsync(CancellationToken cancellationToken = default);
}