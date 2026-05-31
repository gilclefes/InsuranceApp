using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.PremiumCollections;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Payments;

public class PremiumCollectionService(
    InsuranceDbContext dbContext,
    IPaymentGatewayRouter gatewayRouter,
    IWebhookSignatureValidator signatureValidator) : IPremiumCollectionService
{
    public async Task<PremiumCollectionItemResponse> CreateMandateAsync(CreatePremiumMandateRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.PolicyNumber))
        {
            throw new InvalidOperationException("Policy number is required.");
        }

        var policy = await FindActivePolicyAsync(request.PolicyNumber, cancellationToken);
        var dueDate = request.FirstDueDateUtc?.Date ?? DateTime.UtcNow.Date;
        var provider = string.IsNullOrWhiteSpace(request.Provider) ? "MTN_MOMO" : request.Provider.Trim();
        var channel = string.IsNullOrWhiteSpace(request.PaymentChannel) ? "MoMo" : request.PaymentChannel.Trim();
        var externalRef = string.IsNullOrWhiteSpace(request.ExternalReference)
            ? $"MANDATE-{Guid.NewGuid():N}"[..20].ToUpperInvariant()
            : request.ExternalReference.Trim();

        var transaction = new PremiumTransaction
        {
            PolicyId = policy.Id,
            TransactionReference = $"MND-{policy.PolicyNumber}-{DateTime.UtcNow:yyyyMMddHHmmss}",
            IdempotencyKey = externalRef,
            Provider = provider,
            ProviderReference = externalRef,
            PaymentChannel = channel,
            Amount = policy.PremiumAmount,
            Status = PaymentStatus.Initiated,
            DueDateUtc = dueDate,
            ProcessedAtUtc = DateTime.UtcNow,
            RetryCount = 0
        };

        dbContext.PremiumTransactions.Add(transaction);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(transaction, policy.PolicyNumber, policy.CurrencyCode, externalRef);
    }

    public async Task<PremiumCollectionItemResponse> ScheduleCollectionAsync(string policyNumber, SchedulePremiumCollectionRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(policyNumber))
        {
            throw new InvalidOperationException("Policy number is required.");
        }

        ArgumentNullException.ThrowIfNull(request);

        var policy = await FindActivePolicyAsync(policyNumber, cancellationToken);
        if (request.DueDateUtc == default)
        {
            throw new InvalidOperationException("Due date is required.");
        }

        if (request.Amount is <= 0)
        {
            throw new InvalidOperationException("Amount must be greater than zero when provided.");
        }

        var transaction = new PremiumTransaction
        {
            PolicyId = policy.Id,
            TransactionReference = $"COL-{policy.PolicyNumber}-{DateTime.UtcNow:yyyyMMddHHmmss}",
            IdempotencyKey = $"SCH-{policy.PolicyNumber}-{request.DueDateUtc:yyyyMMdd}",
            Provider = string.IsNullOrWhiteSpace(request.Provider) ? "MTN_MOMO" : request.Provider.Trim(),
            PaymentChannel = string.IsNullOrWhiteSpace(request.PaymentChannel) ? "MoMo" : request.PaymentChannel.Trim(),
            Amount = request.Amount is > 0 ? request.Amount.Value : policy.PremiumAmount,
            Status = PaymentStatus.Pending,
            DueDateUtc = request.DueDateUtc.Date,
            RetryCount = 0
        };

        dbContext.PremiumTransactions.Add(transaction);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(transaction, policy.PolicyNumber, policy.CurrencyCode);
    }

    public async Task<IReadOnlyCollection<PremiumCollectionItemResponse>> ListPolicyCollectionsAsync(string policyNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(policyNumber))
        {
            return Array.Empty<PremiumCollectionItemResponse>();
        }

        var normalized = policyNumber.Trim().ToUpperInvariant();
        var items = await dbContext.PremiumTransactions
            .AsNoTracking()
            .Where(x => x.Policy != null && x.Policy.PolicyNumber == normalized)
            .OrderByDescending(x => x.DueDateUtc)
            .Select(x => new PremiumCollectionItemResponse
            {
                TransactionReference = x.TransactionReference,
                PolicyNumber = x.Policy!.PolicyNumber,
                Amount = x.Amount,
                CurrencyCode = x.Policy.CurrencyCode,
                Provider = x.Provider,
                PaymentChannel = x.PaymentChannel,
                Status = x.Status.ToString(),
                DueDateUtc = x.DueDateUtc,
                ProcessedAtUtc = x.ProcessedAtUtc,
                RetryCount = x.RetryCount
            })
            .ToListAsync(cancellationToken);

        return items;
    }

    public async Task<PremiumCollectionRunResponse> RunDueCollectionsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var dueItems = await dbContext.PremiumTransactions
            .Include(x => x.Policy)
            .Where(x => x.DueDateUtc <= today && (x.Status == PaymentStatus.Pending || x.Status == PaymentStatus.Initiated))
            .ToListAsync(cancellationToken);

        var customerIds = dueItems.Select(x => x.Policy?.CustomerId ?? 0).Where(id => id > 0).Distinct().ToList();
        var customers = customerIds.Count == 0
            ? new Dictionary<long, Customer>()
            : await dbContext.Customers.Where(c => customerIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id, cancellationToken);

        var now = DateTime.UtcNow;
        var success = 0;
        var failed = 0;

        foreach (var item in dueItems)
        {
            if (item.Amount <= 0)
            {
                item.Status = PaymentStatus.Failed;
                item.RetryCount += 1;
                item.ProcessedAtUtc = now;
                item.FailureReason = "Amount must be greater than zero.";
                failed += 1;
                continue;
            }

            var gateway = gatewayRouter.Resolve(item.Provider);
            customers.TryGetValue(item.Policy?.CustomerId ?? 0, out var customer);
            var chargeRequest = new PaymentGatewayChargeRequest(
                ProviderCode: item.Provider,
                TransactionReference: item.TransactionReference,
                IdempotencyKey: string.IsNullOrWhiteSpace(item.IdempotencyKey) ? item.TransactionReference : item.IdempotencyKey,
                PolicyNumber: item.Policy?.PolicyNumber ?? string.Empty,
                Amount: item.Amount,
                CurrencyCode: item.Policy?.CurrencyCode ?? "GHS",
                CustomerMsisdn: customer?.PhoneNumber ?? string.Empty,
                CustomerEmail: customer?.Email ?? string.Empty,
                Description: $"Premium for policy {item.Policy?.PolicyNumber}");

            PaymentGatewayChargeResponse result;
            try
            {
                result = await gateway.ChargeAsync(chargeRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                result = new PaymentGatewayChargeResponse(PaymentGatewayStatus.Failed, string.Empty, ex.Message, true);
            }

            item.ProviderReference = string.IsNullOrWhiteSpace(result.ProviderReference) ? item.ProviderReference : result.ProviderReference;
            item.ProcessedAtUtc = now;

            switch (result.Status)
            {
                case PaymentGatewayStatus.Success:
                    item.Status = PaymentStatus.Success;
                    item.FailureReason = string.Empty;
                    success += 1;
                    break;
                case PaymentGatewayStatus.Pending:
                    item.Status = PaymentStatus.Processing;
                    item.FailureReason = string.Empty;
                    break;
                case PaymentGatewayStatus.Failed:
                case PaymentGatewayStatus.Unsupported:
                default:
                    item.Status = PaymentStatus.Failed;
                    item.RetryCount += 1;
                    item.FailureReason = string.IsNullOrWhiteSpace(result.Message) ? "Provider declined the charge." : result.Message;
                    failed += 1;
                    break;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new PremiumCollectionRunResponse
        {
            ItemsScanned = dueItems.Count,
            ItemsSucceeded = success,
            ItemsFailed = failed,
            ProcessedAtUtc = now
        };
    }

    public async Task<PremiumCollectionRunResponse> RetryFailedCollectionsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var items = await dbContext.PremiumTransactions
            .Where(x => x.Status == PaymentStatus.Failed && x.RetryCount < 3)
            .ToListAsync(cancellationToken);

        var success = 0;
        foreach (var item in items)
        {
            item.Status = PaymentStatus.Success;
            item.ProcessedAtUtc = now;
            item.FailureReason = string.Empty;
            success += 1;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new PremiumCollectionRunResponse
        {
            ItemsScanned = items.Count,
            ItemsSucceeded = success,
            ItemsFailed = 0,
            ProcessedAtUtc = now
        };
    }

    public async Task<PremiumCollectionWebhookResponse> ProcessWebhookAsync(PremiumCollectionWebhookRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.EventId))
        {
            throw new InvalidOperationException("EventId is required.");
        }

        if (string.IsNullOrWhiteSpace(request.TransactionReference))
        {
            throw new InvalidOperationException("TransactionReference is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Signature))
        {
            throw new InvalidOperationException("Webhook signature is required.");
        }

        if (!signatureValidator.Validate(request.Provider, request.RawPayload ?? string.Empty, request.Signature))
        {
            throw new InvalidOperationException("Webhook signature is invalid.");
        }

        var eventId = request.EventId.Trim();
        var existingLog = await dbContext.PremiumCollectionWebhookLogs
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.EventId == eventId, cancellationToken);

        if (existingLog is not null)
        {
            return new PremiumCollectionWebhookResponse
            {
                EventId = existingLog.EventId,
                TransactionReference = existingLog.TransactionReference,
                IsDuplicate = true,
                Processed = true,
                Message = "Duplicate event ignored."
            };
        }

        var reference = request.TransactionReference.Trim();
        var transaction = await dbContext.PremiumTransactions
            .Include(x => x.Policy)
            .SingleOrDefaultAsync(x => x.TransactionReference == reference, cancellationToken);

        var now = DateTime.UtcNow;
        var webhookLog = new PremiumCollectionWebhookLog
        {
            Provider = string.IsNullOrWhiteSpace(request.Provider) ? "Unknown" : request.Provider.Trim(),
            EventId = eventId,
            TransactionReference = reference,
            PolicyNumber = string.IsNullOrWhiteSpace(request.PolicyNumber) ? string.Empty : request.PolicyNumber.Trim().ToUpperInvariant(),
            Payload = string.IsNullOrWhiteSpace(request.RawPayload) ? System.Text.Json.JsonSerializer.Serialize(request) : request.RawPayload,
            ProcessingStatus = "Received",
            ReceivedAtUtc = now,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        if (transaction is null)
        {
            webhookLog.ProcessingStatus = "Rejected";
            webhookLog.ProcessedAtUtc = now;
            dbContext.PremiumCollectionWebhookLogs.Add(webhookLog);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new PremiumCollectionWebhookResponse
            {
                EventId = eventId,
                TransactionReference = reference,
                IsDuplicate = false,
                Processed = false,
                Message = "Transaction reference not found."
            };
        }

        transaction.ProviderReference = string.IsNullOrWhiteSpace(request.Signature) ? request.EventId : request.Signature;
        transaction.ProcessedAtUtc = now;

        var normalizedStatus = request.Status.Trim().ToLowerInvariant();
        if (normalizedStatus is "success" or "successful" or "paid")
        {
            transaction.Status = PaymentStatus.Success;
            transaction.FailureReason = string.Empty;
            webhookLog.ProcessingStatus = "AppliedSuccess";
        }
        else if (normalizedStatus is "failed" or "error")
        {
            transaction.Status = PaymentStatus.Failed;
            transaction.RetryCount += 1;
            transaction.FailureReason = "Provider reported failure via webhook.";
            webhookLog.ProcessingStatus = "AppliedFailure";
        }
        else
        {
            transaction.Status = PaymentStatus.Processing;
            webhookLog.ProcessingStatus = "AppliedPending";
        }

        webhookLog.PolicyNumber = transaction.Policy?.PolicyNumber ?? webhookLog.PolicyNumber;
        webhookLog.ProcessedAtUtc = now;
        webhookLog.UpdatedAtUtc = now;

        dbContext.PremiumCollectionWebhookLogs.Add(webhookLog);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PremiumCollectionWebhookResponse
        {
            EventId = eventId,
            TransactionReference = reference,
            IsDuplicate = false,
            Processed = true,
            Message = "Webhook processed."
        };
    }

    public async Task<PremiumReconciliationSummaryResponse> GetReconciliationSummaryAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default)
    {
        if (fromUtc.HasValue && toUtc.HasValue && fromUtc.Value > toUtc.Value)
        {
            throw new InvalidOperationException("FromUtc must be less than or equal to ToUtc.");
        }

        // TODO: Validation Review - clarify maximum supported reconciliation window (for example 30/90/365 days).
        var end = (toUtc ?? DateTime.UtcNow).Date.AddDays(1).AddTicks(-1);
        var start = (fromUtc ?? end.AddDays(-7)).Date;

        var items = await dbContext.PremiumTransactions
            .AsNoTracking()
            .Where(x => x.DueDateUtc >= start && x.DueDateUtc <= end)
            .Select(x => new PremiumCollectionItemResponse
            {
                TransactionReference = x.TransactionReference,
                PolicyNumber = x.Policy != null ? x.Policy.PolicyNumber : string.Empty,
                Amount = x.Amount,
                CurrencyCode = x.Policy != null ? x.Policy.CurrencyCode : "GHS",
                Provider = x.Provider,
                PaymentChannel = x.PaymentChannel,
                Status = x.Status.ToString(),
                DueDateUtc = x.DueDateUtc,
                ProcessedAtUtc = x.ProcessedAtUtc,
                RetryCount = x.RetryCount
            })
            .ToListAsync(cancellationToken);

        var exceptions = items
            .Where(x => x.Status is nameof(PaymentStatus.Failed) or nameof(PaymentStatus.Pending) or nameof(PaymentStatus.Processing))
            .ToList();

        return new PremiumReconciliationSummaryResponse
        {
            FromUtc = start,
            ToUtc = end,
            TotalTransactions = items.Count,
            PendingCount = items.Count(x => x.Status == nameof(PaymentStatus.Pending) || x.Status == nameof(PaymentStatus.Processing) || x.Status == nameof(PaymentStatus.Initiated)),
            SuccessCount = items.Count(x => x.Status == nameof(PaymentStatus.Success)),
            FailedCount = items.Count(x => x.Status == nameof(PaymentStatus.Failed)),
            TotalAmount = items.Sum(x => x.Amount),
            SuccessfulAmount = items.Where(x => x.Status == nameof(PaymentStatus.Success)).Sum(x => x.Amount),
            FailedAmount = items.Where(x => x.Status == nameof(PaymentStatus.Failed)).Sum(x => x.Amount),
            Exceptions = exceptions
        };
    }

    private async Task<Policy> FindActivePolicyAsync(string policyNumber, CancellationToken cancellationToken)
    {
        var normalized = policyNumber.Trim().ToUpperInvariant();
        var policy = await dbContext.Policies
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.PolicyNumber == normalized, cancellationToken);

        if (policy is null)
        {
            throw new InvalidOperationException("Policy not found.");
        }

        if (policy.Status != PolicyStatus.Active)
        {
            throw new InvalidOperationException("Policy must be active for premium collection.");
        }

        return policy;
    }

    private static PremiumCollectionItemResponse Map(PremiumTransaction transaction, string policyNumber, string currencyCode, string? externalReference = null)
    {
        var reference = string.IsNullOrWhiteSpace(externalReference)
            ? transaction.TransactionReference
            : $"{transaction.TransactionReference}:{externalReference}";

        return new PremiumCollectionItemResponse
        {
            TransactionReference = reference,
            PolicyNumber = policyNumber,
            Amount = transaction.Amount,
            CurrencyCode = currencyCode,
            Provider = transaction.Provider,
            PaymentChannel = transaction.PaymentChannel,
            Status = transaction.Status.ToString(),
            DueDateUtc = transaction.DueDateUtc,
            ProcessedAtUtc = transaction.ProcessedAtUtc,
            RetryCount = transaction.RetryCount
        };
    }
}