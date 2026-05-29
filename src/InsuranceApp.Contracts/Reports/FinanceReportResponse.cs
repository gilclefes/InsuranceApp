namespace InsuranceApp.Contracts.Reports;

public sealed class FinanceReportResponse
{
    public DateTime GeneratedAtUtc { get; set; }
    public int TotalPolicies { get; set; }
    public int ActivePolicies { get; set; }
    public int LapsedPolicies { get; set; }
    public int CancelledPolicies { get; set; }
    public decimal LapseRatio { get; set; }
    public IReadOnlyCollection<ClaimsAgingBucket> ClaimsAging { get; set; } = Array.Empty<ClaimsAgingBucket>();
}

public sealed class ClaimsAgingBucket
{
    public string Bucket { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalClaimedAmount { get; set; }
}
