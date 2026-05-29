using InsuranceApp.Contracts.Compliance;
using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Web.Models;

public sealed class ComplianceAdminViewModel
{
    [DataType(DataType.Date)]
    public DateTime? FromUtc { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ToUtc { get; set; }

    public ComplianceSummaryResponse? Summary { get; set; }
}