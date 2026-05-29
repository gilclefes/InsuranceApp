using System.Diagnostics;
using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Operations;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Operations;

public class OperationsHardeningService(InsuranceDbContext dbContext, IDataRetentionService dataRetentionService) : IOperationsHardeningService
{
    public async Task<OperationsReadinessSummaryResponse> GetReadinessSummaryAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var probeWatch = Stopwatch.StartNew();
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        _ = await dbContext.Policies.AsNoTracking().CountAsync(cancellationToken);
        probeWatch.Stop();

        var pendingPremium = await dbContext.PremiumTransactions
            .AsNoTracking()
            .CountAsync(x => x.Status == PaymentStatus.Pending || x.Status == PaymentStatus.Initiated || x.Status == PaymentStatus.Processing, cancellationToken);

        var failedPremium = await dbContext.PremiumTransactions
            .AsNoTracking()
            .CountAsync(x => x.Status == PaymentStatus.Failed, cancellationToken);

        var pendingPayout = await dbContext.PayoutTransactions
            .AsNoTracking()
            .CountAsync(x => x.Status == PaymentStatus.Initiated || x.Status == PaymentStatus.Processing, cancellationToken);

        var failedPayout = await dbContext.PayoutTransactions
            .AsNoTracking()
            .CountAsync(x => x.Status == PaymentStatus.Failed, cancellationToken);

        var openClaims = await dbContext.Claims
            .AsNoTracking()
            .CountAsync(x => x.Status == ClaimStatus.Filed || x.Status == ClaimStatus.UnderReview || x.Status == ClaimStatus.AwaitingDocuments, cancellationToken);

        var lastAuditAt = await dbContext.IdentityAuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => (DateTime?)x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var retentionSummary = await dataRetentionService.GetSummaryAsync(cancellationToken);
        var retentionCandidates = retentionSummary.OtpChallengesEligibleForDeletion
            + retentionSummary.RefreshTokensEligibleForDeletion
            + retentionSummary.IdentityAuditRowsEligibleForAnonymization
            + retentionSummary.ClaimNotificationsEligibleForAnonymization
            + retentionSummary.PremiumWebhookPayloadsEligibleForRedaction
            + retentionSummary.PayoutWebhookPayloadsEligibleForRedaction;

        var readyForDrill = canConnect && failedPremium < 500 && failedPayout < 500;

        return new OperationsReadinessSummaryResponse
        {
            GeneratedAtUtc = now,
            DatabaseConnectivityHealthy = canConnect,
            DatabaseProbeLatencyMs = probeWatch.ElapsedMilliseconds,
            PendingPremiumCollections = pendingPremium,
            FailedPremiumCollections = failedPremium,
            PendingPayouts = pendingPayout,
            FailedPayouts = failedPayout,
            OpenClaims = openClaims,
            LastIdentityAuditAtUtc = lastAuditAt,
            RetentionCleanupCandidates = retentionCandidates,
            ReadyForFailoverDrill = readyForDrill
        };
    }

    public async Task<FailoverDrillRunResponse> RunFailoverDrillAsync(CancellationToken cancellationToken = default)
    {
        var startedAt = DateTime.UtcNow;
        var steps = new List<FailoverDrillStepResponse>();

        await RunStep("DatabaseConnectivity", steps, async () =>
        {
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
            {
                throw new InvalidOperationException("Database connectivity probe failed.");
            }

            return "Database connectivity is healthy.";
        });

        await RunStep("CriticalTableRead", steps, async () =>
        {
            var premiumCount = await dbContext.PremiumTransactions.AsNoTracking().CountAsync(cancellationToken);
            var payoutCount = await dbContext.PayoutTransactions.AsNoTracking().CountAsync(cancellationToken);
            var claimCount = await dbContext.Claims.AsNoTracking().CountAsync(cancellationToken);
            return $"Premium={premiumCount}, Payout={payoutCount}, Claims={claimCount}";
        });

        await RunStep("RetentionPreview", steps, async () =>
        {
            var summary = await dataRetentionService.GetSummaryAsync(cancellationToken);
            var total = summary.OtpChallengesEligibleForDeletion
                + summary.RefreshTokensEligibleForDeletion
                + summary.IdentityAuditRowsEligibleForAnonymization
                + summary.ClaimNotificationsEligibleForAnonymization
                + summary.PremiumWebhookPayloadsEligibleForRedaction
                + summary.PayoutWebhookPayloadsEligibleForRedaction;

            return $"Retention candidates currently at {total}.";
        });

        var completedAt = DateTime.UtcNow;
        var success = steps.All(x => x.Success);

        return new FailoverDrillRunResponse
        {
            StartedAtUtc = startedAt,
            CompletedAtUtc = completedAt,
            Successful = success,
            Steps = steps
        };
    }

    private static async Task RunStep(string stepName, IList<FailoverDrillStepResponse> steps, Func<Task<string>> action)
    {
        var watch = Stopwatch.StartNew();

        try
        {
            var message = await action();
            watch.Stop();

            steps.Add(new FailoverDrillStepResponse
            {
                StepName = stepName,
                Success = true,
                Message = message,
                DurationMs = watch.ElapsedMilliseconds
            });
        }
        catch (Exception ex)
        {
            watch.Stop();

            steps.Add(new FailoverDrillStepResponse
            {
                StepName = stepName,
                Success = false,
                Message = ex.Message,
                DurationMs = watch.ElapsedMilliseconds
            });
        }
    }
}