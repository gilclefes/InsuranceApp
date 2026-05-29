using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Payouts;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Payouts;

public class PayoutService(InsuranceDbContext dbContext) : IPayoutService
{
    public async Task<PayoutResponse> InitiatePayoutAsync(InitiatePayoutRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ClaimNumber))
        {
            throw new InvalidOperationException("Claim number is required.");
        }

        if (string.IsNullOrWhiteSpace(request.DestinationAccount))
        {
            throw new InvalidOperationException("Destination account is required.");
        }

        var claimNumber = request.ClaimNumber.Trim().ToUpperInvariant();
        var claim = await dbContext.Claims
            .Include(x => x.Policy)
            .ThenInclude(x => x!.Customer)
            .SingleOrDefaultAsync(x => x.ClaimNumber == claimNumber, cancellationToken)
            ?? throw new InvalidOperationException("Claim not found.");

        if (claim.Status != ClaimStatus.Approved)
        {
            throw new InvalidOperationException("Only approved claims can be paid out.");
        }

        var amount = request.Amount ?? claim.ApprovedAmount ?? claim.ClaimedAmount;
        if (amount <= 0)
        {
            throw new InvalidOperationException("Payout amount must be greater than zero.");
        }

        var idempotency = string.IsNullOrWhiteSpace(request.ExternalReference)
            ? $"PAY-{claimNumber}-{DateTime.UtcNow:yyyyMMddHHmmss}"
            : request.ExternalReference.Trim();

        var existing = await dbContext.PayoutTransactions
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotency, cancellationToken);
        if (existing is not null)
        {
            return Map(existing, claimNumber);
        }

        var now = DateTime.UtcNow;
        var payout = new PayoutTransaction
        {
            ClaimId = claim.Id,
            PayoutReference = $"PO-{claimNumber}-{now:yyyyMMddHHmmss}",
            IdempotencyKey = idempotency,
            Amount = amount,
            Status = PaymentStatus.Initiated,
            DestinationChannel = string.IsNullOrWhiteSpace(request.DestinationChannel) ? "MoMo" : request.DestinationChannel.Trim(),
            DestinationAccount = request.DestinationAccount.Trim(),
            ProviderReference = string.Empty,
            FailureReason = string.Empty,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        dbContext.PayoutTransactions.Add(payout);
        dbContext.ClaimTimelineEvents.Add(new ClaimTimelineEvent
        {
            ClaimId = claim.Id,
            EventType = "PayoutInitiated",
            Description = $"Payout {payout.PayoutReference} initiated for {amount:N2}.",
            ActorUserId = "System",
            EventAtUtc = now,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });

        AddClaimNotification(claim.Id, claim.Policy?.Customer?.Email, $"Payout for claim {claim.ClaimNumber} has been initiated.", now);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(payout, claimNumber);
    }

    public async Task<PayoutResponse> GetPayoutAsync(string payoutReference, CancellationToken cancellationToken = default)
    {
        var payout = await dbContext.PayoutTransactions
            .AsNoTracking()
            .Include(x => x.Claim)
            .SingleOrDefaultAsync(x => x.PayoutReference == payoutReference.Trim().ToUpperInvariant(), cancellationToken)
            ?? throw new InvalidOperationException("Payout not found.");

        return Map(payout, payout.Claim?.ClaimNumber ?? string.Empty);
    }

    public async Task<IReadOnlyCollection<PayoutResponse>> ListPayoutsAsync(string? status, string? claimNumber, DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default)
    {
        var query = dbContext.PayoutTransactions.AsNoTracking().Include(x => x.Claim).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentStatus>(status, true, out var parsed))
        {
            query = query.Where(x => x.Status == parsed);
        }

        if (!string.IsNullOrWhiteSpace(claimNumber))
        {
            var normalized = claimNumber.Trim().ToUpperInvariant();
            query = query.Where(x => x.Claim != null && x.Claim.ClaimNumber == normalized);
        }

        if (fromUtc.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc <= toUtc.Value);
        }

        var items = await query.OrderByDescending(x => x.CreatedAtUtc).ToListAsync(cancellationToken);
        return items.Select(x => Map(x, x.Claim?.ClaimNumber ?? string.Empty)).ToList();
    }

    public async Task<PayoutReconciliationRunResponse> RunReconciliationAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var pending = await dbContext.PayoutTransactions
            .Include(x => x.Claim)
            .ThenInclude(x => x!.Policy)
            .ThenInclude(x => x!.Customer)
            .Where(x => x.Status == PaymentStatus.Initiated || x.Status == PaymentStatus.Processing)
            .ToListAsync(cancellationToken);

        var success = 0;
        var failed = 0;

        foreach (var item in pending)
        {
            var shouldFail = item.Amount % 2 != 0;
            if (shouldFail)
            {
                item.Status = PaymentStatus.Failed;
                item.FailureReason = "Provider reconciliation marked payout as failed.";
                item.UpdatedAtUtc = now;
                failed += 1;

                dbContext.ClaimTimelineEvents.Add(new ClaimTimelineEvent
                {
                    ClaimId = item.ClaimId,
                    EventType = "PayoutFailed",
                    Description = $"Payout {item.PayoutReference} failed during reconciliation.",
                    ActorUserId = "System",
                    EventAtUtc = now,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });

                AddClaimNotification(item.ClaimId, item.Claim?.Policy?.Customer?.Email, $"Payout {item.PayoutReference} failed and will be retried.", now);
            }
            else
            {
                item.Status = PaymentStatus.Disbursed;
                item.DisbursedAtUtc = now;
                item.FailureReason = string.Empty;
                item.UpdatedAtUtc = now;
                success += 1;

                if (item.Claim is not null)
                {
                    item.Claim.Status = ClaimStatus.Disbursed;
                    item.Claim.UpdatedAtUtc = now;
                }

                dbContext.ClaimTimelineEvents.Add(new ClaimTimelineEvent
                {
                    ClaimId = item.ClaimId,
                    EventType = "PayoutDisbursed",
                    Description = $"Payout {item.PayoutReference} disbursed.",
                    ActorUserId = "System",
                    EventAtUtc = now,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });

                AddClaimNotification(item.ClaimId, item.Claim?.Policy?.Customer?.Email, $"Payout {item.PayoutReference} has been disbursed.", now);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new PayoutReconciliationRunResponse
        {
            ItemsScanned = pending.Count,
            ReconciledCount = success,
            FailedCount = failed,
            ProcessedAtUtc = now
        };
    }

    public async Task<PayoutWebhookResponse> ProcessWebhookAsync(PayoutWebhookRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.EventId))
        {
            throw new InvalidOperationException("EventId is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Signature))
        {
            throw new InvalidOperationException("Webhook signature is required.");
        }

        var eventId = request.EventId.Trim();
        var existingLog = await dbContext.PayoutWebhookLogs.AsNoTracking().SingleOrDefaultAsync(x => x.EventId == eventId, cancellationToken);
        if (existingLog is not null)
        {
            return new PayoutWebhookResponse
            {
                EventId = eventId,
                PayoutReference = existingLog.PayoutReference,
                IsDuplicate = true,
                Processed = true,
                Message = "Duplicate event ignored."
            };
        }

        var payoutReference = request.PayoutReference.Trim().ToUpperInvariant();
        var payout = await dbContext.PayoutTransactions
            .Include(x => x.Claim)
            .ThenInclude(x => x!.Policy)
            .ThenInclude(x => x!.Customer)
            .SingleOrDefaultAsync(x => x.PayoutReference == payoutReference, cancellationToken);

        var now = DateTime.UtcNow;
        var log = new PayoutWebhookLog
        {
            Provider = string.IsNullOrWhiteSpace(request.Provider) ? "Unknown" : request.Provider.Trim(),
            EventId = eventId,
            PayoutReference = payoutReference,
            Payload = string.IsNullOrWhiteSpace(request.RawPayload) ? System.Text.Json.JsonSerializer.Serialize(request) : request.RawPayload,
            ProcessingStatus = "Received",
            ReceivedAtUtc = now,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        if (payout is null)
        {
            log.ProcessingStatus = "Rejected";
            log.ProcessedAtUtc = now;
            dbContext.PayoutWebhookLogs.Add(log);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new PayoutWebhookResponse
            {
                EventId = eventId,
                PayoutReference = payoutReference,
                IsDuplicate = false,
                Processed = false,
                Message = "Payout reference not found."
            };
        }

        var normalized = request.Status.Trim().ToLowerInvariant();
        payout.ProviderReference = request.Signature.Trim();
        payout.UpdatedAtUtc = now;

        if (normalized is "success" or "disbursed" or "paid")
        {
            payout.Status = PaymentStatus.Disbursed;
            payout.DisbursedAtUtc = now;
            payout.FailureReason = string.Empty;
            if (payout.Claim is not null)
            {
                payout.Claim.Status = ClaimStatus.Disbursed;
                payout.Claim.UpdatedAtUtc = now;
            }

            dbContext.ClaimTimelineEvents.Add(new ClaimTimelineEvent
            {
                ClaimId = payout.ClaimId,
                EventType = "PayoutDisbursed",
                Description = $"Payout {payout.PayoutReference} confirmed by provider webhook.",
                ActorUserId = "System",
                EventAtUtc = now,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });

            AddClaimNotification(payout.ClaimId, payout.Claim?.Policy?.Customer?.Email, $"Payout {payout.PayoutReference} has been confirmed as disbursed.", now);
            log.ProcessingStatus = "AppliedSuccess";
        }
        else if (normalized is "failed" or "error")
        {
            payout.Status = PaymentStatus.Failed;
            payout.FailureReason = "Provider webhook reported failure.";

            dbContext.ClaimTimelineEvents.Add(new ClaimTimelineEvent
            {
                ClaimId = payout.ClaimId,
                EventType = "PayoutFailed",
                Description = $"Payout {payout.PayoutReference} failed by provider webhook.",
                ActorUserId = "System",
                EventAtUtc = now,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });

            AddClaimNotification(payout.ClaimId, payout.Claim?.Policy?.Customer?.Email, $"Payout {payout.PayoutReference} failed. Support will follow up.", now);
            log.ProcessingStatus = "AppliedFailure";
        }
        else
        {
            payout.Status = PaymentStatus.Processing;
            log.ProcessingStatus = "AppliedPending";
        }

        log.ProcessedAtUtc = now;
        log.UpdatedAtUtc = now;

        dbContext.PayoutWebhookLogs.Add(log);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PayoutWebhookResponse
        {
            EventId = eventId,
            PayoutReference = payoutReference,
            IsDuplicate = false,
            Processed = true,
            Message = "Webhook processed."
        };
    }

    private static PayoutResponse Map(PayoutTransaction payout, string claimNumber)
    {
        return new PayoutResponse
        {
            PayoutReference = payout.PayoutReference,
            ClaimNumber = claimNumber,
            Amount = payout.Amount,
            Status = payout.Status.ToString(),
            DestinationChannel = payout.DestinationChannel,
            DestinationAccount = payout.DestinationAccount,
            ProviderReference = payout.ProviderReference,
            FailureReason = payout.FailureReason,
            CreatedAtUtc = payout.CreatedAtUtc,
            DisbursedAtUtc = payout.DisbursedAtUtc
        };
    }

    private void AddClaimNotification(long claimId, string? recipient, string message, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(recipient))
        {
            return;
        }

        dbContext.ClaimNotifications.Add(new ClaimNotification
        {
            ClaimId = claimId,
            Channel = "Email",
            Recipient = recipient.Trim(),
            Message = message,
            Status = "Sent",
            SentAtUtc = now,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });
    }
}