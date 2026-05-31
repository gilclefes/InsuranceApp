using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Contracts.Claims;

public sealed class CreateClaimRequest
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(40)]
    public string PolicyNumber { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateTime IncidentDate { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(80)]
    public string ClaimType { get; set; } = string.Empty;

    [Range(0.01, 100000000)]
    public decimal ClaimedAmount { get; set; }

    [Url]
    [StringLength(512)]
    public string EvidenceUrl { get; set; } = string.Empty;
}