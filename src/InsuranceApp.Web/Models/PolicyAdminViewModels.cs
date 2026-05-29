using InsuranceApp.Contracts.Policies;
using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Web.Models;

public sealed class PolicyDashboardViewModel
{
    public string? StatusFilter { get; set; }
    public IReadOnlyCollection<PolicySummaryResponse> Policies { get; set; } = Array.Empty<PolicySummaryResponse>();
}

public sealed class CreatePolicyViewModel
{
    [Required]
    public string QuoteReference { get; set; } = string.Empty;

    [Required]
    public long CustomerId { get; set; }

    public string AssignedAgentId { get; set; } = string.Empty;

    [Required]
    public string CoverageType { get; set; } = "Standard";

    [Required]
    [DataType(DataType.Date)]
    public DateTime InceptionDate { get; set; } = DateTime.UtcNow.Date;

    [Required]
    [DataType(DataType.Date)]
    public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.Date.AddYears(1);
}