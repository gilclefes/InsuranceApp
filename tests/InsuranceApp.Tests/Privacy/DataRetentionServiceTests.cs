using InsuranceApp.Domain.Entities;
using InsuranceApp.Infrastructure.Persistence;
using InsuranceApp.Infrastructure.Privacy;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Tests.Privacy;

public class DataRetentionServiceTests
{
    [Fact]
    public async Task GetSummaryAsync_ShouldReturnEligibleCounts()
    {
        await using var context = CreateDbContext(nameof(GetSummaryAsync_ShouldReturnEligibleCounts));
        SeedRetentionData(context);
        await context.SaveChangesAsync();

        var service = new DataRetentionService(context);
        var summary = await service.GetSummaryAsync();

        Assert.True(summary.OtpChallengesEligibleForDeletion > 0);
        Assert.True(summary.RefreshTokensEligibleForDeletion > 0);
        Assert.True(summary.IdentityAuditRowsEligibleForAnonymization > 0);
        Assert.True(summary.ClaimNotificationsEligibleForAnonymization > 0);
        Assert.True(summary.PremiumWebhookPayloadsEligibleForRedaction > 0);
        Assert.True(summary.PayoutWebhookPayloadsEligibleForRedaction > 0);
    }

    [Fact]
    public async Task RunRetentionAsync_ShouldDeleteAndAnonymizeTargets()
    {
        await using var context = CreateDbContext(nameof(RunRetentionAsync_ShouldDeleteAndAnonymizeTargets));
        SeedRetentionData(context);
        await context.SaveChangesAsync();

        var service = new DataRetentionService(context);
        var result = await service.RunRetentionAsync();

        Assert.True(result.DeletedOtpChallenges > 0);
        Assert.True(result.DeletedRefreshTokens > 0);
        Assert.True(result.AnonymizedIdentityAuditRows > 0);
        Assert.True(result.AnonymizedClaimNotifications > 0);
        Assert.True(result.RedactedPremiumWebhookPayloads > 0);
        Assert.True(result.RedactedPayoutWebhookPayloads > 0);

        Assert.Equal(0, await context.OtpChallenges.CountAsync());
        Assert.Equal(0, await context.RefreshTokens.CountAsync());

        var audit = await context.IdentityAuditLogs.SingleAsync();
        Assert.StartsWith("anon-", audit.SubjectId, StringComparison.Ordinal);
        Assert.Equal("REDACTED", audit.IpAddress);
        Assert.Equal("REDACTED", audit.UserAgent);

        var claimNotification = await context.ClaimNotifications.SingleAsync();
        Assert.Equal("REDACTED", claimNotification.Recipient);
        Assert.Equal("REDACTED", claimNotification.Message);

        var premiumWebhook = await context.PremiumCollectionWebhookLogs.SingleAsync();
        Assert.Equal("REDACTED", premiumWebhook.Payload);

        var payoutWebhook = await context.PayoutWebhookLogs.SingleAsync();
        Assert.Equal("REDACTED", payoutWebhook.Payload);
    }

    private static void SeedRetentionData(InsuranceDbContext context)
    {
        var now = DateTime.UtcNow;

        context.OtpChallenges.Add(new OtpChallenge
        {
            ChallengeId = "OTP-1",
            Destination = "+233240000000",
            Purpose = "Login",
            Channel = "SMS",
            CodeHash = "hash",
            ExpiresAtUtc = now.AddDays(-40),
            MaxAttempts = 3,
            AttemptCount = 1,
            LastSentAtUtc = now.AddDays(-41),
            VerifiedAtUtc = now.AddDays(-40),
            CreatedAtUtc = now.AddDays(-41),
            UpdatedAtUtc = now.AddDays(-41)
        });

        context.RefreshTokens.Add(new RefreshToken
        {
            Token = "refresh-token-1",
            UserId = "user-1",
            SessionId = "session-1",
            DeviceId = "device-1",
            IpAddress = "127.0.0.1",
            UserAgent = "Test",
            ExpiresAtUtc = now.AddDays(-45),
            RevokedAtUtc = now.AddDays(-40),
            CreatedAtUtc = now.AddDays(-60),
            UpdatedAtUtc = now.AddDays(-40)
        });

        context.IdentityAuditLogs.Add(new IdentityAuditLog
        {
            Action = "Login",
            Outcome = "Failure",
            SubjectId = "agent-1",
            Description = "Invalid password",
            CorrelationId = "corr-1",
            IpAddress = "10.0.0.1",
            UserAgent = "Mozilla",
            CreatedAtUtc = now.AddDays(-200),
            UpdatedAtUtc = now.AddDays(-200)
        });

        context.ClaimNotifications.Add(new ClaimNotification
        {
            ClaimId = 1,
            Channel = "Email",
            Recipient = "user@example.com",
            Message = "Your claim was updated",
            Status = "Sent",
            SentAtUtc = now.AddDays(-130),
            CreatedAtUtc = now.AddDays(-130),
            UpdatedAtUtc = now.AddDays(-130)
        });

        context.PremiumCollectionWebhookLogs.Add(new PremiumCollectionWebhookLog
        {
            Provider = "MTN_MOMO",
            EventId = "PEV-1",
            TransactionReference = "TX-1",
            PolicyNumber = "POL-1",
            Payload = "{ raw: 'premium' }",
            ProcessingStatus = "AppliedSuccess",
            ReceivedAtUtc = now.AddDays(-70),
            ProcessedAtUtc = now.AddDays(-70),
            CreatedAtUtc = now.AddDays(-70),
            UpdatedAtUtc = now.AddDays(-70)
        });

        context.PayoutWebhookLogs.Add(new PayoutWebhookLog
        {
            Provider = "MOCK",
            EventId = "POEV-1",
            PayoutReference = "PO-1",
            Payload = "{ raw: 'payout' }",
            ProcessingStatus = "AppliedSuccess",
            ReceivedAtUtc = now.AddDays(-70),
            ProcessedAtUtc = now.AddDays(-70),
            CreatedAtUtc = now.AddDays(-70),
            UpdatedAtUtc = now.AddDays(-70)
        });
    }

    private static InsuranceDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<InsuranceDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var context = new InsuranceDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}