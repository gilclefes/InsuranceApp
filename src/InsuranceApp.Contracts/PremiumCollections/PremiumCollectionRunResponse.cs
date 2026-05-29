namespace InsuranceApp.Contracts.PremiumCollections;

public sealed class PremiumCollectionRunResponse
{
    public int ItemsScanned { get; set; }
    public int ItemsSucceeded { get; set; }
    public int ItemsFailed { get; set; }
    public DateTime ProcessedAtUtc { get; set; }
}