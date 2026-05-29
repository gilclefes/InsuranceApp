namespace InsuranceApp.Contracts.Privacy;

public sealed class DataRetentionSummaryResponse
{
    public DateTime GeneratedAtUtc { get; set; }

    public int OtpRetentionDays { get; set; }
    public DateTime OtpCutoffUtc { get; set; }
    public int OtpChallengesEligibleForDeletion { get; set; }

    public int RefreshTokenRetentionDays { get; set; }
    public DateTime RefreshTokenCutoffUtc { get; set; }
    public int RefreshTokensEligibleForDeletion { get; set; }

    public int IdentityAuditPiiRetentionDays { get; set; }
    public DateTime IdentityAuditCutoffUtc { get; set; }
    public int IdentityAuditRowsEligibleForAnonymization { get; set; }

    public int ClaimNotificationPiiRetentionDays { get; set; }
    public DateTime ClaimNotificationCutoffUtc { get; set; }
    public int ClaimNotificationsEligibleForAnonymization { get; set; }

    public int WebhookPayloadRetentionDays { get; set; }
    public DateTime WebhookPayloadCutoffUtc { get; set; }
    public int PremiumWebhookPayloadsEligibleForRedaction { get; set; }
    public int PayoutWebhookPayloadsEligibleForRedaction { get; set; }
}