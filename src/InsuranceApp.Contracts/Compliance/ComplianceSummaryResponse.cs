namespace InsuranceApp.Contracts.Compliance;

public sealed class ComplianceSummaryResponse
{
    public DateTime FromUtc { get; set; }
    public DateTime ToUtc { get; set; }

    public int PremiumTotalTransactions { get; set; }
    public int PremiumSuccessfulTransactions { get; set; }
    public int PremiumFailedTransactions { get; set; }
    public decimal PremiumCollectedAmount { get; set; }
    public decimal PremiumFailedAmount { get; set; }
    public decimal PremiumSuccessRate { get; set; }

    public int PayoutTotalTransactions { get; set; }
    public int PayoutSuccessfulTransactions { get; set; }
    public int PayoutFailedTransactions { get; set; }
    public decimal PayoutDisbursedAmount { get; set; }
    public decimal PayoutFailedAmount { get; set; }
    public decimal PayoutSuccessRate { get; set; }

    public int ClaimsTotal { get; set; }
    public int ClaimsApproved { get; set; }
    public int ClaimsRejected { get; set; }
    public int ClaimsFraudFlagged { get; set; }
    public int ClaimsSlaBreached { get; set; }

    public int IdentityAuditTotal { get; set; }
    public int IdentityAuditFailures { get; set; }

    public IReadOnlyCollection<ComplianceExceptionItemResponse> Exceptions { get; set; } = Array.Empty<ComplianceExceptionItemResponse>();
}