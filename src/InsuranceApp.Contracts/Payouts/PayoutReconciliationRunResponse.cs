namespace InsuranceApp.Contracts.Payouts;

public sealed class PayoutReconciliationRunResponse
{
    public int ItemsScanned { get; set; }
    public int ReconciledCount { get; set; }
    public int FailedCount { get; set; }
    public DateTime ProcessedAtUtc { get; set; }
}