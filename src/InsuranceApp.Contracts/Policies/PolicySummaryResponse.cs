namespace InsuranceApp.Contracts.Policies;

public sealed class PolicySummaryResponse
{
    public string PolicyNumber { get; set; } = string.Empty;
    public long CustomerId { get; set; }
    public string AssignedAgentId { get; set; } = string.Empty;
    public string ProductType { get; set; } = string.Empty;
    public string CoverageType { get; set; } = string.Empty;
    public decimal PremiumAmount { get; set; }
    public string CurrencyCode { get; set; } = "GHS";
    public DateTime InceptionDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string DocumentReference { get; set; } = string.Empty;
}
