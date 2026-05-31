using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Contracts.Claims;

public sealed class ReviewClaimRequest
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(32)]
    public string Action { get; set; } = string.Empty;

    [Range(0.01, 100000000)]
    public decimal? ApprovedAmount { get; set; }

    [StringLength(250)]
    public string Reason { get; set; } = string.Empty;
}