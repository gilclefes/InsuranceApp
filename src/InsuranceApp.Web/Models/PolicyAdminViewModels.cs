using InsuranceApp.Contracts.Policies;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Web.Models;

public sealed class PolicyDashboardViewModel
{
    public string? StatusFilter { get; set; }
    public IReadOnlyCollection<PolicySummaryResponse> Policies { get; set; } = Array.Empty<PolicySummaryResponse>();
    public IReadOnlyDictionary<long, string> CustomerDisplayById { get; set; } = new Dictionary<long, string>();
    public IReadOnlyDictionary<string, string> AgentDisplayById { get; set; } = new Dictionary<string, string>();
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

    public IFormFile? SupportingDocument { get; set; }

    public IReadOnlyCollection<LookupOption> CustomerOptions { get; set; } = Array.Empty<LookupOption>();
    public IReadOnlyCollection<LookupOption> AgentOptions { get; set; } = Array.Empty<LookupOption>();
}

public sealed class LookupOption
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}