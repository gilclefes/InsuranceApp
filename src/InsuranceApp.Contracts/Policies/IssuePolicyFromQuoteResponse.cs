namespace InsuranceApp.Contracts.Policies;

public sealed class IssuePolicyFromQuoteResponse
{
    public string PolicyNumber { get; set; } = string.Empty;
    public string QuoteReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal PremiumAmount { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string DocumentReference { get; set; } = string.Empty;
}
