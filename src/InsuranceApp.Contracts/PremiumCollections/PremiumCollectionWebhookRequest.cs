namespace InsuranceApp.Contracts.PremiumCollections;

public sealed class PremiumCollectionWebhookRequest
{
    public string Provider { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string TransactionReference { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public DateTime EventTimeUtc { get; set; }
    public string Signature { get; set; } = string.Empty;
    public string RawPayload { get; set; } = string.Empty;
}