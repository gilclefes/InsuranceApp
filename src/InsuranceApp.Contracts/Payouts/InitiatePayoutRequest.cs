namespace InsuranceApp.Contracts.Payouts;

public sealed class InitiatePayoutRequest
{
    public string ClaimNumber { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public string DestinationChannel { get; set; } = "MoMo";
    public string DestinationAccount { get; set; } = string.Empty;
    public string ExternalReference { get; set; } = string.Empty;
}