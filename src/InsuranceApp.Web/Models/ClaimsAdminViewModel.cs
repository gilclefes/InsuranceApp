using InsuranceApp.Contracts.Claims;
using Microsoft.AspNetCore.Http;
using InsuranceApp.Web.Models;
using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Web.Models;

public sealed class ClaimsAdminViewModel
{
    public string? StatusFilter { get; set; }
    public string? AdjusterFilter { get; set; }
    public string? PolicyFilter { get; set; }

    [Required(AllowEmptyStrings = false)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Policy number cannot be empty.")]
    [StringLength(40)]
    public string PolicyNumber { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateTime IncidentDate { get; set; } = DateTime.UtcNow.Date;

    [Required(AllowEmptyStrings = false)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Claim type cannot be empty.")]
    [StringLength(80)]
    public string ClaimType { get; set; } = "Accident";

    [Range(1, 100000000)]
    public decimal ClaimedAmount { get; set; }

    [Url]
    [StringLength(512)]
    public string EvidenceUrl { get; set; } = string.Empty;
    public IFormFile? EvidenceDocument { get; set; }

    [StringLength(40)]
    public string AssignClaimNumber { get; set; } = string.Empty;

    [StringLength(64)]
    public string AssignAdjusterUserId { get; set; } = string.Empty;

    [StringLength(250)]
    public string AssignNote { get; set; } = string.Empty;

    [StringLength(40)]
    public string ReviewClaimNumber { get; set; } = string.Empty;

    [StringLength(32)]
    public string ReviewAction { get; set; } = "under-review";

    [Range(0.01, 100000000)]
    public decimal? ReviewApprovedAmount { get; set; }

    [StringLength(250)]
    public string ReviewReason { get; set; } = string.Empty;

    [StringLength(40)]
    public string TimelineClaimNumber { get; set; } = string.Empty;

    public IReadOnlyCollection<ClaimResponse> Claims { get; set; } = Array.Empty<ClaimResponse>();
    public IReadOnlyCollection<ClaimTimelineEventResponse> Timeline { get; set; } = Array.Empty<ClaimTimelineEventResponse>();
    public ClaimSlaDashboardResponse? Dashboard { get; set; }
    public IReadOnlyCollection<LookupOption> AdjusterOptions { get; set; } = Array.Empty<LookupOption>();
    public IReadOnlyDictionary<string, string> AdjusterDisplayById { get; set; } = new Dictionary<string, string>();
}