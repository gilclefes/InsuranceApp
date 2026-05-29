namespace InsuranceApp.Contracts.PremiumCollections;

public sealed class PremiumCollectionItemResponse
{
    public string TransactionReference { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "GHS";
    public string Provider { get; set; } = string.Empty;
    public string PaymentChannel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DueDateUtc { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
    public int RetryCount { get; set; }
}