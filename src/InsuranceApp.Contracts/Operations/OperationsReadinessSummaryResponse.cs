namespace InsuranceApp.Contracts.Operations;

public sealed class OperationsReadinessSummaryResponse
{
    public DateTime GeneratedAtUtc { get; set; }
    public bool DatabaseConnectivityHealthy { get; set; }
    public long DatabaseProbeLatencyMs { get; set; }
    public int PendingPremiumCollections { get; set; }
    public int FailedPremiumCollections { get; set; }
    public int PendingPayouts { get; set; }
    public int FailedPayouts { get; set; }
    public int OpenClaims { get; set; }
    public DateTime? LastIdentityAuditAtUtc { get; set; }
    public int RetentionCleanupCandidates { get; set; }
    public bool ReadyForFailoverDrill { get; set; }
}