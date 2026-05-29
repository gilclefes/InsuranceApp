using InsuranceApp.Contracts.Operations;

namespace InsuranceApp.Web.Models;

public sealed class OperationsAdminViewModel
{
    public OperationsReadinessSummaryResponse? Readiness { get; set; }
    public FailoverDrillRunResponse? LastDrillResult { get; set; }
}