namespace InsuranceApp.Contracts.Privacy;

public sealed class DataRetentionRunResponse
{
    public DateTime ProcessedAtUtc { get; set; }
    public int DeletedOtpChallenges { get; set; }
    public int DeletedRefreshTokens { get; set; }
    public int AnonymizedIdentityAuditRows { get; set; }
    public int AnonymizedClaimNotifications { get; set; }
    public int RedactedPremiumWebhookPayloads { get; set; }
    public int RedactedPayoutWebhookPayloads { get; set; }
}