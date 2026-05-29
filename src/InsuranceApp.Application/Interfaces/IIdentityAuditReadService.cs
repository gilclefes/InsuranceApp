using InsuranceApp.Contracts.Audit;

namespace InsuranceApp.Application.Interfaces;

public interface IIdentityAuditReadService
{
    Task<IdentityAuditLogQueryResponse> QueryAsync(IdentityAuditLogQueryRequest request, CancellationToken cancellationToken = default);
    Task<string> ExportCsvAsync(IdentityAuditLogQueryRequest request, CancellationToken cancellationToken = default);
}
