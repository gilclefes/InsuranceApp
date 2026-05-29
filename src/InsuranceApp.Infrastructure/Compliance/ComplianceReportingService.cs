using System.Text;
using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Compliance;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Compliance;

public class ComplianceReportingService(InsuranceDbContext dbContext) : IComplianceReportingService
{
    private const int ClaimsSlaHours = 72;

    public async Task<ComplianceSummaryResponse> GetSummaryAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default)
    {
        var (start, end) = NormalizeRange(fromUtc, toUtc);

        var premiums = await dbContext.PremiumTransactions
            .AsNoTracking()
            .Where(x => x.CreatedAtUtc >= start && x.CreatedAtUtc <= end)
            .Select(x => new { x.TransactionReference, x.Status, x.Amount, x.FailureReason, x.ProcessedAtUtc, x.CreatedAtUtc })
            .ToListAsync(cancellationToken);

        var payouts = await dbContext.PayoutTransactions
            .AsNoTracking()
            .Where(x => x.CreatedAtUtc >= start && x.CreatedAtUtc <= end)
            .Select(x => new { x.PayoutReference, x.Status, x.Amount, x.FailureReason, x.DisbursedAtUtc, x.UpdatedAtUtc, x.CreatedAtUtc })
            .ToListAsync(cancellationToken);

        var claims = await dbContext.Claims
            .AsNoTracking()
            .Where(x => x.CreatedAtUtc >= start && x.CreatedAtUtc <= end)
            .Select(x => new { x.ClaimNumber, x.Status, x.ApprovedAmount, x.ClaimedAmount, x.IsFraudRisk, x.FraudReason, x.CreatedAtUtc, x.UpdatedAtUtc })
            .ToListAsync(cancellationToken);

        var auditLogs = await dbContext.IdentityAuditLogs
            .AsNoTracking()
            .Where(x => x.CreatedAtUtc >= start && x.CreatedAtUtc <= end)
            .Select(x => new { x.Action, x.Outcome, x.SubjectId, x.Description, x.CreatedAtUtc })
            .ToListAsync(cancellationToken);

        var premiumSuccess = premiums.Count(x => x.Status == PaymentStatus.Success);
        var premiumFailed = premiums.Count(x => x.Status == PaymentStatus.Failed);

        var payoutSuccess = payouts.Count(x => x.Status == PaymentStatus.Disbursed || x.Status == PaymentStatus.Success);
        var payoutFailed = payouts.Count(x => x.Status == PaymentStatus.Failed);

        var now = DateTime.UtcNow;
        var openStatuses = new[] { ClaimStatus.Filed, ClaimStatus.UnderReview, ClaimStatus.AwaitingDocuments };
        var claimsBreached = claims.Count(x => openStatuses.Contains(x.Status) && (now - x.CreatedAtUtc).TotalHours > ClaimsSlaHours);

        var exceptions = BuildExceptions(premiums, payouts, claims, auditLogs)
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(100)
            .ToArray();

        return new ComplianceSummaryResponse
        {
            FromUtc = start,
            ToUtc = end,
            PremiumTotalTransactions = premiums.Count,
            PremiumSuccessfulTransactions = premiumSuccess,
            PremiumFailedTransactions = premiumFailed,
            PremiumCollectedAmount = premiums.Where(x => x.Status == PaymentStatus.Success).Sum(x => x.Amount),
            PremiumFailedAmount = premiums.Where(x => x.Status == PaymentStatus.Failed).Sum(x => x.Amount),
            PremiumSuccessRate = premiums.Count == 0 ? 0 : Math.Round((decimal)premiumSuccess / premiums.Count * 100m, 2),

            PayoutTotalTransactions = payouts.Count,
            PayoutSuccessfulTransactions = payoutSuccess,
            PayoutFailedTransactions = payoutFailed,
            PayoutDisbursedAmount = payouts.Where(x => x.Status == PaymentStatus.Disbursed || x.Status == PaymentStatus.Success).Sum(x => x.Amount),
            PayoutFailedAmount = payouts.Where(x => x.Status == PaymentStatus.Failed).Sum(x => x.Amount),
            PayoutSuccessRate = payouts.Count == 0 ? 0 : Math.Round((decimal)payoutSuccess / payouts.Count * 100m, 2),

            ClaimsTotal = claims.Count,
            ClaimsApproved = claims.Count(x => x.Status == ClaimStatus.Approved || x.Status == ClaimStatus.Disbursed || x.Status == ClaimStatus.Closed),
            ClaimsRejected = claims.Count(x => x.Status == ClaimStatus.Rejected),
            ClaimsFraudFlagged = claims.Count(x => x.IsFraudRisk),
            ClaimsSlaBreached = claimsBreached,

            IdentityAuditTotal = auditLogs.Count,
            IdentityAuditFailures = auditLogs.Count(x => !string.Equals(x.Outcome, "Success", StringComparison.OrdinalIgnoreCase)),
            Exceptions = exceptions
        };
    }

    public async Task<string> ExportSummaryCsvAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(fromUtc, toUtc, cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine("Section,Metric,Value");
        builder.AppendLine($"Range,FromUtc,{Escape(summary.FromUtc.ToString("O"))}");
        builder.AppendLine($"Range,ToUtc,{Escape(summary.ToUtc.ToString("O"))}");

        builder.AppendLine($"Premiums,TotalTransactions,{summary.PremiumTotalTransactions}");
        builder.AppendLine($"Premiums,SuccessfulTransactions,{summary.PremiumSuccessfulTransactions}");
        builder.AppendLine($"Premiums,FailedTransactions,{summary.PremiumFailedTransactions}");
        builder.AppendLine($"Premiums,CollectedAmount,{summary.PremiumCollectedAmount}");
        builder.AppendLine($"Premiums,FailedAmount,{summary.PremiumFailedAmount}");
        builder.AppendLine($"Premiums,SuccessRate,{summary.PremiumSuccessRate}");

        builder.AppendLine($"Payouts,TotalTransactions,{summary.PayoutTotalTransactions}");
        builder.AppendLine($"Payouts,SuccessfulTransactions,{summary.PayoutSuccessfulTransactions}");
        builder.AppendLine($"Payouts,FailedTransactions,{summary.PayoutFailedTransactions}");
        builder.AppendLine($"Payouts,DisbursedAmount,{summary.PayoutDisbursedAmount}");
        builder.AppendLine($"Payouts,FailedAmount,{summary.PayoutFailedAmount}");
        builder.AppendLine($"Payouts,SuccessRate,{summary.PayoutSuccessRate}");

        builder.AppendLine($"Claims,Total,{summary.ClaimsTotal}");
        builder.AppendLine($"Claims,ApprovedOrClosed,{summary.ClaimsApproved}");
        builder.AppendLine($"Claims,Rejected,{summary.ClaimsRejected}");
        builder.AppendLine($"Claims,FraudFlagged,{summary.ClaimsFraudFlagged}");
        builder.AppendLine($"Claims,SlaBreached,{summary.ClaimsSlaBreached}");

        builder.AppendLine($"IdentityAudit,Total,{summary.IdentityAuditTotal}");
        builder.AppendLine($"IdentityAudit,Failures,{summary.IdentityAuditFailures}");

        builder.AppendLine();
        builder.AppendLine("Exceptions");
        builder.AppendLine("Category,Reference,Status,Amount,Reason,OccurredAtUtc");

        foreach (var item in summary.Exceptions)
        {
            builder.AppendLine(string.Join(',',
                Escape(item.Category),
                Escape(item.Reference),
                Escape(item.Status),
                item.Amount?.ToString() ?? string.Empty,
                Escape(item.Reason),
                Escape(item.OccurredAtUtc.ToString("O"))));
        }

        return builder.ToString();
    }

    private static IEnumerable<ComplianceExceptionItemResponse> BuildExceptions(
        IEnumerable<dynamic> premiums,
        IEnumerable<dynamic> payouts,
        IEnumerable<dynamic> claims,
        IEnumerable<dynamic> auditLogs)
    {
        foreach (var premium in premiums.Where(x => x.Status == PaymentStatus.Failed))
        {
            yield return new ComplianceExceptionItemResponse
            {
                Category = "Premium",
                Reference = premium.TransactionReference,
                Status = premium.Status.ToString(),
                Amount = premium.Amount,
                Reason = string.IsNullOrWhiteSpace(premium.FailureReason) ? "Collection failed" : premium.FailureReason,
                OccurredAtUtc = premium.ProcessedAtUtc ?? premium.CreatedAtUtc
            };
        }

        foreach (var payout in payouts.Where(x => x.Status == PaymentStatus.Failed))
        {
            yield return new ComplianceExceptionItemResponse
            {
                Category = "Payout",
                Reference = payout.PayoutReference,
                Status = payout.Status.ToString(),
                Amount = payout.Amount,
                Reason = string.IsNullOrWhiteSpace(payout.FailureReason) ? "Payout failed" : payout.FailureReason,
                OccurredAtUtc = payout.UpdatedAtUtc
            };
        }

        foreach (var claim in claims.Where(x => x.IsFraudRisk))
        {
            yield return new ComplianceExceptionItemResponse
            {
                Category = "ClaimFraud",
                Reference = claim.ClaimNumber,
                Status = claim.Status.ToString(),
                Amount = claim.ApprovedAmount ?? claim.ClaimedAmount,
                Reason = string.IsNullOrWhiteSpace(claim.FraudReason) ? "Fraud risk flagged" : claim.FraudReason,
                OccurredAtUtc = claim.UpdatedAtUtc
            };
        }

        foreach (var log in auditLogs.Where(x => !string.Equals((string)x.Outcome, "Success", StringComparison.OrdinalIgnoreCase)))
        {
            yield return new ComplianceExceptionItemResponse
            {
                Category = "IdentityAudit",
                Reference = log.SubjectId,
                Status = log.Outcome,
                Amount = null,
                Reason = string.IsNullOrWhiteSpace(log.Description) ? log.Action : log.Description,
                OccurredAtUtc = log.CreatedAtUtc
            };
        }
    }

    private static (DateTime start, DateTime end) NormalizeRange(DateTime? fromUtc, DateTime? toUtc)
    {
        var end = (toUtc ?? DateTime.UtcNow).Date.AddDays(1).AddTicks(-1);
        var start = (fromUtc ?? end.AddDays(-30)).Date;

        if (start > end)
        {
            (start, end) = (end.Date, start.Date.AddDays(1).AddTicks(-1));
        }

        return (start, end);
    }

    private static string Escape(string value)
    {
        var normalized = value.Replace("\"", "\"\"");
        return $"\"{normalized}\"";
    }
}