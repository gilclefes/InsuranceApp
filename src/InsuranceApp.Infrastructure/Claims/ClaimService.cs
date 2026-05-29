using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Claims;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Claims;

public class ClaimService(InsuranceDbContext dbContext) : IClaimService
{
    public async Task<ClaimResponse> CreateClaimAsync(CreateClaimRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.PolicyNumber))
        {
            throw new InvalidOperationException("Policy number is required.");
        }

        if (request.ClaimedAmount <= 0)
        {
            throw new InvalidOperationException("Claimed amount must be greater than zero.");
        }

        var policyNumber = request.PolicyNumber.Trim().ToUpperInvariant();
        var policy = await dbContext.Policies
            .Include(x => x.Customer)
            .SingleOrDefaultAsync(x => x.PolicyNumber == policyNumber, cancellationToken)
            ?? throw new InvalidOperationException("Policy not found.");

        var now = DateTime.UtcNow;
        var sequence = await dbContext.Claims.CountAsync(cancellationToken) + 1;
        var claim = new Claim
        {
            ClaimNumber = $"CLM-{now:yyyyMMdd}-{sequence:0000}",
            PolicyId = policy.Id,
            IncidentDate = request.IncidentDate == default ? now.Date : request.IncidentDate.Date,
            ClaimType = string.IsNullOrWhiteSpace(request.ClaimType) ? "General" : request.ClaimType.Trim(),
            ClaimedAmount = request.ClaimedAmount,
            EvidenceUrl = request.EvidenceUrl.Trim(),
            Status = ClaimStatus.Filed,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        var (fraudScore, fraudReason) = EvaluateFraudRisk(claim, policy);
        claim.FraudScore = fraudScore;
        claim.IsFraudRisk = fraudScore >= 60;
        claim.FraudReason = fraudReason;

        dbContext.Claims.Add(claim);
        dbContext.ClaimTimelineEvents.Add(new ClaimTimelineEvent
        {
            Claim = claim,
            EventType = "Created",
            Description = $"Claim submitted for policy {policy.PolicyNumber}.",
            ActorUserId = "System",
            EventAtUtc = now,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });

        AddNotification(claim, policy.Customer?.Email, "Email", $"Claim {claim.ClaimNumber} has been filed and is under processing.", now);
        if (claim.IsFraudRisk)
        {
            AddNotification(claim, "claims-risk@insuranceapp.local", "Email", $"Fraud risk alert for {claim.ClaimNumber}: {claim.FraudReason}", now);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(claim, policy.PolicyNumber, now);
    }

    public async Task<ClaimResponse> GetClaimAsync(string claimNumber, CancellationToken cancellationToken = default)
    {
        var claim = await dbContext.Claims
            .AsNoTracking()
            .Include(x => x.Policy)
            .SingleOrDefaultAsync(x => x.ClaimNumber == claimNumber.Trim().ToUpperInvariant(), cancellationToken)
            ?? throw new InvalidOperationException("Claim not found.");

        return Map(claim, claim.Policy?.PolicyNumber ?? string.Empty, DateTime.UtcNow);
    }

    public async Task<IReadOnlyCollection<ClaimResponse>> ListClaimsAsync(string? status, string? assignedAdjusterId, string? policyNumber, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Claims.AsNoTracking().Include(x => x.Policy).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ClaimStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(x => x.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(assignedAdjusterId))
        {
            var adjuster = assignedAdjusterId.Trim();
            query = query.Where(x => x.AssignedAdjusterId == adjuster);
        }

        if (!string.IsNullOrWhiteSpace(policyNumber))
        {
            var normalizedPolicy = policyNumber.Trim().ToUpperInvariant();
            query = query.Where(x => x.Policy != null && x.Policy.PolicyNumber == normalizedPolicy);
        }

        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        return items.Select(x => Map(x, x.Policy?.PolicyNumber ?? string.Empty, now)).ToList();
    }

    public async Task<ClaimResponse> AssignClaimAsync(string claimNumber, AssignClaimRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.AdjusterUserId))
        {
            throw new InvalidOperationException("Adjuster user id is required.");
        }

        var claim = await dbContext.Claims
            .Include(x => x.Policy)
            .ThenInclude(x => x!.Customer)
            .SingleOrDefaultAsync(x => x.ClaimNumber == claimNumber.Trim().ToUpperInvariant(), cancellationToken)
            ?? throw new InvalidOperationException("Claim not found.");

        claim.AssignedAdjusterId = request.AdjusterUserId.Trim();
        claim.ReviewNotes = string.IsNullOrWhiteSpace(request.Note) ? claim.ReviewNotes : request.Note.Trim();
        claim.Status = claim.Status == ClaimStatus.Filed ? ClaimStatus.UnderReview : claim.Status;
        var now = DateTime.UtcNow;
        claim.UpdatedAtUtc = now;

        dbContext.ClaimTimelineEvents.Add(new ClaimTimelineEvent
        {
            ClaimId = claim.Id,
            EventType = "Assigned",
            Description = $"Claim assigned to {claim.AssignedAdjusterId}.",
            ActorUserId = claim.AssignedAdjusterId,
            EventAtUtc = now,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });

        AddNotification(claim, claim.Policy?.Customer?.Email, "Email", $"Claim {claim.ClaimNumber} is now under review.", now);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(claim, claim.Policy?.PolicyNumber ?? string.Empty, now);
    }

    public async Task<ClaimResponse> ReviewClaimAsync(string claimNumber, ReviewClaimRequest request, CancellationToken cancellationToken = default)
    {
        var claim = await dbContext.Claims
            .Include(x => x.Policy)
            .ThenInclude(x => x!.Customer)
            .SingleOrDefaultAsync(x => x.ClaimNumber == claimNumber.Trim().ToUpperInvariant(), cancellationToken)
            ?? throw new InvalidOperationException("Claim not found.");

        var action = request.Action.Trim().ToLowerInvariant();
        switch (action)
        {
            case "approve":
                if (request.ApprovedAmount is null || request.ApprovedAmount <= 0)
                {
                    throw new InvalidOperationException("Approved amount is required for approve action.");
                }

                if (request.ApprovedAmount > claim.ClaimedAmount)
                {
                    throw new InvalidOperationException("Approved amount cannot exceed claimed amount.");
                }
                claim.ApprovedAmount = request.ApprovedAmount;
                claim.Status = ClaimStatus.Approved;
                claim.DecisionReason = string.IsNullOrWhiteSpace(request.Reason) ? "Approved" : request.Reason.Trim();
                claim.DecisionedAtUtc = DateTime.UtcNow;
                break;
            case "reject":
                claim.ApprovedAmount = null;
                claim.Status = ClaimStatus.Rejected;
                claim.DecisionReason = string.IsNullOrWhiteSpace(request.Reason) ? "Rejected" : request.Reason.Trim();
                claim.DecisionedAtUtc = DateTime.UtcNow;
                break;
            case "request-documents":
                claim.Status = ClaimStatus.AwaitingDocuments;
                claim.DecisionReason = string.IsNullOrWhiteSpace(request.Reason) ? "Additional documents requested." : request.Reason.Trim();
                break;
            case "under-review":
                claim.Status = ClaimStatus.UnderReview;
                claim.DecisionReason = string.IsNullOrWhiteSpace(request.Reason) ? claim.DecisionReason : request.Reason.Trim();
                break;
            case "close":
                claim.Status = ClaimStatus.Closed;
                claim.DecisionReason = string.IsNullOrWhiteSpace(request.Reason) ? "Claim closed." : request.Reason.Trim();
                claim.DecisionedAtUtc = DateTime.UtcNow;
                break;
            default:
                throw new InvalidOperationException("Unsupported review action.");
        }

        if (!string.IsNullOrWhiteSpace(request.Reason))
        {
            claim.ReviewNotes = request.Reason.Trim();
        }

        var now = DateTime.UtcNow;
        claim.UpdatedAtUtc = now;

        dbContext.ClaimTimelineEvents.Add(new ClaimTimelineEvent
        {
            ClaimId = claim.Id,
            EventType = "Review",
            Description = $"Action '{action}' applied with status {claim.Status}.",
            ActorUserId = claim.AssignedAdjusterId,
            EventAtUtc = now,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });

        AddNotification(claim, claim.Policy?.Customer?.Email, "Email", $"Claim {claim.ClaimNumber} status changed to {claim.Status}.", now);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(claim, claim.Policy?.PolicyNumber ?? string.Empty, now);
    }

    public async Task<IReadOnlyCollection<ClaimTimelineEventResponse>> GetTimelineAsync(string claimNumber, CancellationToken cancellationToken = default)
    {
        var normalized = claimNumber.Trim().ToUpperInvariant();
        var claim = await dbContext.Claims
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.ClaimNumber == normalized, cancellationToken)
            ?? throw new InvalidOperationException("Claim not found.");

        var events = await dbContext.ClaimTimelineEvents
            .AsNoTracking()
            .Where(x => x.ClaimId == claim.Id)
            .OrderBy(x => x.EventAtUtc)
            .Select(x => new ClaimTimelineEventResponse
            {
                EventType = x.EventType,
                Description = x.Description,
                ActorUserId = x.ActorUserId,
                EventAtUtc = x.EventAtUtc
            })
            .ToListAsync(cancellationToken);

        return events;
    }

    public async Task<ClaimSlaDashboardResponse> GetSlaDashboardAsync(string? assignedAdjusterId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Claims.AsNoTracking().Include(x => x.Policy).Where(x =>
            x.Status == ClaimStatus.Filed ||
            x.Status == ClaimStatus.UnderReview ||
            x.Status == ClaimStatus.AwaitingDocuments);

        if (!string.IsNullOrWhiteSpace(assignedAdjusterId))
        {
            var adjuster = assignedAdjusterId.Trim();
            query = query.Where(x => x.AssignedAdjusterId == adjuster);
        }

        var now = DateTime.UtcNow;
        var openClaims = await query.OrderByDescending(x => x.CreatedAtUtc).ToListAsync(cancellationToken);
        var mapped = openClaims.Select(x => Map(x, x.Policy?.PolicyNumber ?? string.Empty, now)).ToList();

        var breached = mapped.Count(x => x.IsSlaBreached);
        var total = mapped.Count;
        var breachPct = total == 0 ? 0 : Math.Round((decimal)breached / total * 100, 2);

        return new ClaimSlaDashboardResponse
        {
            TotalOpenClaims = total,
            BreachedClaims = breached,
            HighFraudRiskClaims = mapped.Count(x => x.IsFraudRisk),
            BreachPercentage = breachPct,
            OpenQueue = mapped
        };
    }

    private static ClaimResponse Map(Claim claim, string policyNumber, DateTime nowUtc)
    {
        var ageHours = (int)Math.Floor((nowUtc - claim.CreatedAtUtc).TotalHours);
        var isOpen = claim.Status is ClaimStatus.Filed or ClaimStatus.UnderReview or ClaimStatus.AwaitingDocuments;
        return new ClaimResponse
        {
            ClaimNumber = claim.ClaimNumber,
            PolicyNumber = policyNumber,
            IncidentDate = claim.IncidentDate,
            ClaimType = claim.ClaimType,
            ClaimedAmount = claim.ClaimedAmount,
            ApprovedAmount = claim.ApprovedAmount,
            EvidenceUrl = claim.EvidenceUrl,
            AssignedAdjusterId = claim.AssignedAdjusterId,
            Status = claim.Status.ToString(),
            DecisionReason = claim.DecisionReason,
            ReviewNotes = claim.ReviewNotes,
            IsFraudRisk = claim.IsFraudRisk,
            FraudScore = claim.FraudScore,
            FraudReason = claim.FraudReason,
            AgeHours = ageHours,
            IsSlaBreached = isOpen && ageHours > 72,
            CreatedAtUtc = claim.CreatedAtUtc,
            DecisionedAtUtc = claim.DecisionedAtUtc
        };
    }

    private static (decimal score, string reason) EvaluateFraudRisk(Claim claim, Policy policy)
    {
        decimal score = 0;
        var reasons = new List<string>();

        if (claim.IncidentDate < policy.InceptionDate)
        {
            score += 50;
            reasons.Add("Incident date precedes policy inception.");
        }

        var policyAgeDays = (claim.IncidentDate.Date - policy.InceptionDate.Date).TotalDays;
        if (policyAgeDays <= 14)
        {
            score += 20;
            reasons.Add("Claim filed within first 14 days of policy.");
        }

        if (policy.PremiumAmount > 0)
        {
            var ratio = claim.ClaimedAmount / policy.PremiumAmount;
            if (ratio >= 20)
            {
                score += 40;
                reasons.Add("Claim amount is unusually high relative to premium.");
            }
            else if (ratio >= 10)
            {
                score += 25;
                reasons.Add("Claim amount is high relative to premium.");
            }
        }

        if (string.IsNullOrWhiteSpace(claim.EvidenceUrl))
        {
            score += 15;
            reasons.Add("No evidence submitted.");
        }

        return (Math.Min(score, 100), reasons.Count == 0 ? "No major fraud indicators." : string.Join(" ", reasons));
    }

    private void AddNotification(Claim claim, string? recipient, string channel, string message, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(recipient))
        {
            return;
        }

        dbContext.ClaimNotifications.Add(new ClaimNotification
        {
            Claim = claim,
            Channel = channel,
            Recipient = recipient.Trim(),
            Message = message,
            Status = "Sent",
            SentAtUtc = now,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });
    }
}