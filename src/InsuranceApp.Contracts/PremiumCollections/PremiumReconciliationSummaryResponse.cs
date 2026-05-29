namespace InsuranceApp.Contracts.PremiumCollections;

public sealed class PremiumReconciliationSummaryResponse
{
    public DateTime FromUtc { get; set; }
    public DateTime ToUtc { get; set; }
    public int TotalTransactions { get; set; }
    public int PendingCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal SuccessfulAmount { get; set; }
    public decimal FailedAmount { get; set; }
    public IReadOnlyCollection<PremiumCollectionItemResponse> Exceptions { get; set; } = Array.Empty<PremiumCollectionItemResponse>();
}