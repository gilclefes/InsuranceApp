using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.PremiumCollections;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Payments;

public class PremiumCollectionService(InsuranceDbContext dbContext) : IPremiumCollectionService
{
    public async Task<PremiumCollectionItemResponse> CreateMandateAsync(CreatePremiumMandateRequest request, CancellationToken cancellationToken = default)
    {
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
            Provider = provider,
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
        var policy = await FindActivePolicyAsync(policyNumber, cancellationToken);
        if (request.DueDateUtc == default)
        {
            throw new InvalidOperationException("Due date is required.");
        }

        var transaction = new PremiumTransaction
        {
            PolicyId = policy.Id,
            TransactionReference = $"COL-{policy.PolicyNumber}-{DateTime.UtcNow:yyyyMMddHHmmss}",
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
            .Where(x => x.DueDateUtc <= today && (x.Status == PaymentStatus.Pending || x.Status == PaymentStatus.Initiated))
            .ToListAsync(cancellationToken);

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
                failed += 1;
                continue;
            }

            var shouldFail = item.RetryCount == 0 && item.Amount % 2 != 0;
            if (shouldFail)
            {
                item.Status = PaymentStatus.Failed;
                item.RetryCount += 1;
                item.ProcessedAtUtc = now;
                failed += 1;
            }
            else
            {
                item.Status = PaymentStatus.Success;
                item.ProcessedAtUtc = now;
                success += 1;
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

    private async Task<Policy> FindActivePolicyAsync(string policyNumber, CancellationToken cancellationToken)
    {
        var normalized = policyNumber.Trim().ToUpperInvariant();
        var policy = await dbContext.Policies
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