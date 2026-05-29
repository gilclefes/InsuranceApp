using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Privacy;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Privacy;

public class DataRetentionService(InsuranceDbContext dbContext) : IDataRetentionService
{
    private const int OtpRetentionDays = 30;
    private const int RefreshTokenRetentionDays = 30;
    private const int IdentityAuditPiiRetentionDays = 180;
    private const int ClaimNotificationPiiRetentionDays = 120;
    private const int WebhookPayloadRetentionDays = 60;

    public async Task<DataRetentionSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var otpCutoff = now.AddDays(-OtpRetentionDays);
        var tokenCutoff = now.AddDays(-RefreshTokenRetentionDays);
        var auditCutoff = now.AddDays(-IdentityAuditPiiRetentionDays);
        var claimNotificationCutoff = now.AddDays(-ClaimNotificationPiiRetentionDays);
        var webhookCutoff = now.AddDays(-WebhookPayloadRetentionDays);

        var otpDeleteCount = await dbContext.OtpChallenges
            .AsNoTracking()
            .CountAsync(x => x.CreatedAtUtc < otpCutoff && (x.VerifiedAtUtc != null || x.ExpiresAtUtc < now), cancellationToken);

        var refreshDeleteCount = await dbContext.RefreshTokens
            .AsNoTracking()
            .CountAsync(x => (x.RevokedAtUtc != null && x.RevokedAtUtc < tokenCutoff) || x.ExpiresAtUtc < tokenCutoff, cancellationToken);

        var auditAnonymizeCount = await dbContext.IdentityAuditLogs
            .AsNoTracking()
            .CountAsync(x => x.CreatedAtUtc < auditCutoff && (x.IpAddress != "REDACTED" || x.UserAgent != "REDACTED" || !x.SubjectId.StartsWith("anon-")), cancellationToken);

        var claimNotificationAnonymizeCount = await dbContext.ClaimNotifications
            .AsNoTracking()
            .CountAsync(x => x.SentAtUtc < claimNotificationCutoff && (x.Recipient != "REDACTED" || x.Message != "REDACTED"), cancellationToken);

        var premiumWebhookRedactCount = await dbContext.PremiumCollectionWebhookLogs
            .AsNoTracking()
            .CountAsync(x => x.ReceivedAtUtc < webhookCutoff && x.Payload != "REDACTED", cancellationToken);

        var payoutWebhookRedactCount = await dbContext.PayoutWebhookLogs
            .AsNoTracking()
            .CountAsync(x => x.ReceivedAtUtc < webhookCutoff && x.Payload != "REDACTED", cancellationToken);

        return new DataRetentionSummaryResponse
        {
            GeneratedAtUtc = now,
            OtpRetentionDays = OtpRetentionDays,
            OtpCutoffUtc = otpCutoff,
            OtpChallengesEligibleForDeletion = otpDeleteCount,
            RefreshTokenRetentionDays = RefreshTokenRetentionDays,
            RefreshTokenCutoffUtc = tokenCutoff,
            RefreshTokensEligibleForDeletion = refreshDeleteCount,
            IdentityAuditPiiRetentionDays = IdentityAuditPiiRetentionDays,
            IdentityAuditCutoffUtc = auditCutoff,
            IdentityAuditRowsEligibleForAnonymization = auditAnonymizeCount,
            ClaimNotificationPiiRetentionDays = ClaimNotificationPiiRetentionDays,
            ClaimNotificationCutoffUtc = claimNotificationCutoff,
            ClaimNotificationsEligibleForAnonymization = claimNotificationAnonymizeCount,
            WebhookPayloadRetentionDays = WebhookPayloadRetentionDays,
            WebhookPayloadCutoffUtc = webhookCutoff,
            PremiumWebhookPayloadsEligibleForRedaction = premiumWebhookRedactCount,
            PayoutWebhookPayloadsEligibleForRedaction = payoutWebhookRedactCount
        };
    }

    public async Task<DataRetentionRunResponse> RunRetentionAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var otpCutoff = now.AddDays(-OtpRetentionDays);
        var tokenCutoff = now.AddDays(-RefreshTokenRetentionDays);
        var auditCutoff = now.AddDays(-IdentityAuditPiiRetentionDays);
        var claimNotificationCutoff = now.AddDays(-ClaimNotificationPiiRetentionDays);
        var webhookCutoff = now.AddDays(-WebhookPayloadRetentionDays);

        var otpToDelete = await dbContext.OtpChallenges
            .Where(x => x.CreatedAtUtc < otpCutoff && (x.VerifiedAtUtc != null || x.ExpiresAtUtc < now))
            .ToListAsync(cancellationToken);

        var tokensToDelete = await dbContext.RefreshTokens
            .Where(x => (x.RevokedAtUtc != null && x.RevokedAtUtc < tokenCutoff) || x.ExpiresAtUtc < tokenCutoff)
            .ToListAsync(cancellationToken);

        var auditsToAnonymize = await dbContext.IdentityAuditLogs
            .Where(x => x.CreatedAtUtc < auditCutoff && (x.IpAddress != "REDACTED" || x.UserAgent != "REDACTED" || !x.SubjectId.StartsWith("anon-")))
            .ToListAsync(cancellationToken);

        var claimNotificationsToAnonymize = await dbContext.ClaimNotifications
            .Where(x => x.SentAtUtc < claimNotificationCutoff && (x.Recipient != "REDACTED" || x.Message != "REDACTED"))
            .ToListAsync(cancellationToken);

        var premiumWebhookPayloadsToRedact = await dbContext.PremiumCollectionWebhookLogs
            .Where(x => x.ReceivedAtUtc < webhookCutoff && x.Payload != "REDACTED")
            .ToListAsync(cancellationToken);

        var payoutWebhookPayloadsToRedact = await dbContext.PayoutWebhookLogs
            .Where(x => x.ReceivedAtUtc < webhookCutoff && x.Payload != "REDACTED")
            .ToListAsync(cancellationToken);

        if (otpToDelete.Count > 0)
        {
            dbContext.OtpChallenges.RemoveRange(otpToDelete);
        }

        if (tokensToDelete.Count > 0)
        {
            dbContext.RefreshTokens.RemoveRange(tokensToDelete);
        }

        foreach (var item in auditsToAnonymize)
        {
            item.SubjectId = $"anon-{item.Id}";
            item.IpAddress = "REDACTED";
            item.UserAgent = "REDACTED";
            item.UpdatedAtUtc = now;
        }

        foreach (var item in claimNotificationsToAnonymize)
        {
            item.Recipient = "REDACTED";
            item.Message = "REDACTED";
            item.UpdatedAtUtc = now;
        }

        foreach (var item in premiumWebhookPayloadsToRedact)
        {
            item.Payload = "REDACTED";
            item.UpdatedAtUtc = now;
        }

        foreach (var item in payoutWebhookPayloadsToRedact)
        {
            item.Payload = "REDACTED";
            item.UpdatedAtUtc = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new DataRetentionRunResponse
        {
            ProcessedAtUtc = now,
            DeletedOtpChallenges = otpToDelete.Count,
            DeletedRefreshTokens = tokensToDelete.Count,
            AnonymizedIdentityAuditRows = auditsToAnonymize.Count,
            AnonymizedClaimNotifications = claimNotificationsToAnonymize.Count,
            RedactedPremiumWebhookPayloads = premiumWebhookPayloadsToRedact.Count,
            RedactedPayoutWebhookPayloads = payoutWebhookPayloadsToRedact.Count
        };
    }
}