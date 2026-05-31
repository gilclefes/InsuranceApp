using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Contracts.Payouts;

public sealed class InitiatePayoutRequest
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(40)]
    public string ClaimNumber { get; set; } = string.Empty;

    [Range(0.01, 100000000)]
    public decimal? Amount { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(32)]
    public string DestinationChannel { get; set; } = "MoMo";

    [Required(AllowEmptyStrings = false)]
    [StringLength(120)]
    public string DestinationAccount { get; set; } = string.Empty;

    [StringLength(80)]
    public string ExternalReference { get; set; } = string.Empty;
}