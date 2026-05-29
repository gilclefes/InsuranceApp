namespace InsuranceApp.Contracts.Policies;

public sealed class IssuePolicyFromQuoteRequest
{
    public string QuoteReference { get; set; } = string.Empty;
    public long CustomerId { get; set; }
    public string AssignedAgentId { get; set; } = string.Empty;
    public string CoverageType { get; set; } = string.Empty;
    public DateTime InceptionDate { get; set; }
    public DateTime ExpiryDate { get; set; }
}
