namespace InsuranceApp.Contracts.Payouts;

public sealed class PayoutWebhookResponse
{
    public string EventId { get; set; } = string.Empty;
    public string PayoutReference { get; set; } = string.Empty;
    public bool IsDuplicate { get; set; }
    public bool Processed { get; set; }
    public string Message { get; set; } = string.Empty;
}