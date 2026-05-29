namespace InsuranceApp.Contracts.Policies;

public sealed class EndorsePolicyRequest
{
    public string CoverageType { get; set; } = string.Empty;
    public decimal PremiumAdjustment { get; set; }
    public string Reason { get; set; } = string.Empty;
}
