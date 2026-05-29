using InsuranceApp.Contracts.Reports;

namespace InsuranceApp.Application.Interfaces;

public interface IFinanceReportingService
{
    Task<FinanceReportResponse> GetFinanceReportAsync(CancellationToken cancellationToken = default);
}
