namespace InsuranceApp.Contracts.Payouts;

public sealed class PayoutWebhookRequest
{
    public string Provider { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string PayoutReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string RawPayload { get; set; } = string.Empty;
    public DateTime EventTimeUtc { get; set; }
}