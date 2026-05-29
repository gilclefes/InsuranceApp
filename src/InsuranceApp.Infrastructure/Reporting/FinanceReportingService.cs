using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Reports;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Reporting;

public class FinanceReportingService(InsuranceDbContext dbContext) : IFinanceReportingService
{
    public async Task<FinanceReportResponse> GetFinanceReportAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var statusCounts = await dbContext.Policies
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var active = statusCounts.FirstOrDefault(x => x.Status == PolicyStatus.Active)?.Count ?? 0;
        var lapsed = statusCounts.FirstOrDefault(x => x.Status == PolicyStatus.Lapsed)?.Count ?? 0;
        var cancelled = statusCounts.FirstOrDefault(x => x.Status == PolicyStatus.Cancelled)?.Count ?? 0;
        var total = statusCounts.Sum(x => x.Count);
        var denominator = active + lapsed;
        var lapseRatio = denominator == 0 ? 0m : Math.Round((decimal)lapsed / denominator, 4);

        var openClaims = await dbContext.Claims
            .Where(c => c.Status == ClaimStatus.Filed
                     || c.Status == ClaimStatus.UnderReview
                     || c.Status == ClaimStatus.AwaitingDocuments
                     || c.Status == ClaimStatus.Approved)
            .Select(c => new { c.CreatedAtUtc, c.ClaimedAmount })
            .ToListAsync(cancellationToken);

        var buckets = new[]
        {
            new { Label = "0-7 days", Min = 0, Max = 7 },
            new { Label = "8-30 days", Min = 8, Max = 30 },
            new { Label = "31-60 days", Min = 31, Max = 60 },
            new { Label = "61-90 days", Min = 61, Max = 90 },
            new { Label = "90+ days", Min = 91, Max = int.MaxValue }
        };

        var aging = buckets.Select(b =>
        {
            var subset = openClaims.Where(c =>
            {
                var age = (int)(now - c.CreatedAtUtc).TotalDays;
                return age >= b.Min && age <= b.Max;
            }).ToList();

            return new ClaimsAgingBucket
            {
                Bucket = b.Label,
                Count = subset.Count,
                TotalClaimedAmount = subset.Sum(s => s.ClaimedAmount)
            };
        }).ToList();

        return new FinanceReportResponse
        {
            GeneratedAtUtc = now,
            TotalPolicies = total,
            ActivePolicies = active,
            LapsedPolicies = lapsed,
            CancelledPolicies = cancelled,
            LapseRatio = lapseRatio,
            ClaimsAging = aging
        };
    }
}
