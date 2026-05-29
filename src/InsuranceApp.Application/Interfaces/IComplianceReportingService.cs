using InsuranceApp.Contracts.Compliance;

namespace InsuranceApp.Application.Interfaces;

public interface IComplianceReportingService
{
    Task<ComplianceSummaryResponse> GetSummaryAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default);
    Task<string> ExportSummaryCsvAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default);
}