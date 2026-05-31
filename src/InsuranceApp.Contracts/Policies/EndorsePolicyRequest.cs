using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Contracts.Policies;

public sealed class EndorsePolicyRequest
{
    [StringLength(100)]
    public string CoverageType { get; set; } = string.Empty;

    [Range(-100000000, 100000000)]
    public decimal PremiumAdjustment { get; set; }

    [StringLength(250)]
    public string Reason { get; set; } = string.Empty;
}
