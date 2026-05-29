using InsuranceApp.Contracts.Audit;
using InsuranceApp.Contracts.Reports;

namespace InsuranceApp.Web.Models;

public class ReportsFinanceViewModel
{
    public FinanceReportResponse Report { get; set; } = new();
}

public class ReportsAuditViewModel
{
    public IdentityAuditLogQueryRequest Query { get; set; } = new();
    public IdentityAuditLogQueryResponse Result { get; set; } = new();
}
