namespace InsuranceApp.Contracts.PremiumCollections;

public sealed class PremiumCollectionWebhookResponse
{
    public string EventId { get; set; } = string.Empty;
    public string TransactionReference { get; set; } = string.Empty;
    public bool IsDuplicate { get; set; }
    public bool Processed { get; set; }
    public string Message { get; set; } = string.Empty;
}