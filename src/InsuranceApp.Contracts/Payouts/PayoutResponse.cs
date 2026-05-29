namespace InsuranceApp.Contracts.Payouts;

public sealed class PayoutResponse
{
    public string PayoutReference { get; set; } = string.Empty;
    public string ClaimNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string DestinationChannel { get; set; } = string.Empty;
    public string DestinationAccount { get; set; } = string.Empty;
    public string ProviderReference { get; set; } = string.Empty;
    public string FailureReason { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? DisbursedAtUtc { get; set; }
}