using InsuranceApp.Application.Interfaces;
using InsuranceApp.Infrastructure.Caching;
using InsuranceApp.Contracts.Quotes;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace InsuranceApp.Infrastructure.Quotes;

public class RuleBasedQuoteService(InsuranceDbContext dbContext, IMemoryCache? cache = null) : IQuoteService
{
    private readonly IMemoryCache memoryCache = cache ?? new MemoryCache(new MemoryCacheOptions());

    public async Task<GenerateQuoteResponse> GenerateQuoteAsync(GenerateQuoteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await GenerateAndPersistQuoteAsync(request, cancellationToken);
    }

    public async Task<QuoteRecordResponse> GetQuoteAsync(string quoteReference, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(quoteReference))
        {
            throw new InvalidOperationException("Quote reference is required.");
        }

        var quote = await dbContext.QuoteRecords
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.QuoteReference == quoteReference.Trim(), cancellationToken)
            ?? throw new InvalidOperationException("Quote reference not found.");

        return MapQuote(quote);
    }

    public async Task<IReadOnlyCollection<QuoteRecordResponse>> ListQuotesAsync(string? productCode, string? status, DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default)
    {
        if (fromUtc.HasValue && toUtc.HasValue && fromUtc.Value > toUtc.Value)
        {
            throw new InvalidOperationException("FromUtc must be less than or equal to ToUtc.");
        }

        var query = dbContext.QuoteRecords.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(productCode))
        {
            query = query.Where(x => x.ProductCode == productCode);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (fromUtc.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc <= toUtc.Value);
        }

        var rows = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return rows.Select(MapQuote).ToList();
    }

    public async Task<GenerateQuoteResponse> RepriceQuoteAsync(string quoteReference, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(quoteReference))
        {
            throw new InvalidOperationException("Quote reference is required.");
        }

        var current = await dbContext.QuoteRecords
            .SingleOrDefaultAsync(x => x.QuoteReference == quoteReference.Trim(), cancellationToken)
            ?? throw new InvalidOperationException("Quote reference not found.");

        if (string.Equals(current.Status, "Issued", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Issued quotes cannot be repriced.");
        }

        current.Status = "Repriced";
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GenerateAndPersistQuoteAsync(new GenerateQuoteRequest
        {
            ProductCode = current.ProductCode,
            CoverageAmount = current.CoverageAmount,
            ApplicantAge = current.ApplicantAge,
            VehicleValue = current.VehicleValue,
            SumAssured = current.SumAssured,
            SelectedRiderCodes = string.IsNullOrWhiteSpace(current.SelectedRiderCodes)
                ? []
                : current.SelectedRiderCodes.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        }, cancellationToken);
    }

    public async Task<QuoteRecordResponse> ExpireQuoteAsync(string quoteReference, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(quoteReference))
        {
            throw new InvalidOperationException("Quote reference is required.");
        }

        var quote = await dbContext.QuoteRecords
            .SingleOrDefaultAsync(x => x.QuoteReference == quoteReference.Trim(), cancellationToken)
            ?? throw new InvalidOperationException("Quote reference not found.");

        if (string.Equals(quote.Status, "Issued", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Issued quotes cannot be expired.");
        }

        quote.Status = "Expired";
        quote.ValidUntilUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapQuote(quote);
    }

    private async Task<GenerateQuoteResponse> GenerateAndPersistQuoteAsync(GenerateQuoteRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.ProductCode))
        {
            throw new InvalidOperationException("Product code is required.");
        }

        var productCode = request.ProductCode.Trim().ToUpperInvariant();
        var product = await GetActiveProductDefinitionAsync(productCode, cancellationToken)
            ?? throw new InvalidOperationException("Product not found or inactive.");

        var insuredBase = request.CoverageAmount > 0 ? request.CoverageAmount : request.SumAssured;
        if (insuredBase <= 0)
        {
            throw new InvalidOperationException("Coverage amount or sum assured is required.");
        }

        var basePremium = decimal.Round(Math.Max(product.MinPremium, insuredBase * product.BaseRate), 2, MidpointRounding.AwayFromZero);
        var adjustments = new List<QuoteAdjustment>();
        var riderAdjustments = new List<QuoteRiderAdjustment>();

        foreach (var rule in product.RiskRules)
        {
            var parameterValue = ResolveParameter(rule.ParameterName, request);
            if (!Matches(rule.Operator, parameterValue, rule.ThresholdValue))
            {
                continue;
            }

            var adjustment = rule.AdjustmentType.Equals("Flat", StringComparison.OrdinalIgnoreCase)
                ? rule.AdjustmentValue
                : decimal.Round(basePremium * (rule.AdjustmentValue / 100m), 2, MidpointRounding.AwayFromZero);

            adjustments.Add(new QuoteAdjustment
            {
                Reason = string.IsNullOrWhiteSpace(rule.Reason) ? $"Rule applied: {rule.ParameterName}" : rule.Reason,
                Amount = adjustment
            });
        }

        var selectedRiderCodes = request.SelectedRiderCodes
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToUpperInvariant())
            .ToHashSet();
        var selectedRidersSerialized = string.Join(',', selectedRiderCodes.OrderBy(x => x));

        if (selectedRiderCodes.Count > 0)
        {
            var riders = product.Riders
                .Where(x => x.IsActive && selectedRiderCodes.Contains(x.RiderCode))
                .ToList();

            foreach (var rider in riders)
            {
                var amount = rider.AdjustmentType.Equals("Percent", StringComparison.OrdinalIgnoreCase)
                    ? decimal.Round(basePremium * (rider.AdjustmentValue / 100m), 2, MidpointRounding.AwayFromZero)
                    : rider.AdjustmentValue;

                adjustments.Add(new QuoteAdjustment
                {
                    Reason = $"Rider: {rider.Name}",
                    Amount = amount
                });

                riderAdjustments.Add(new QuoteRiderAdjustment
                {
                    RiderCode = rider.RiderCode,
                    Name = rider.Name,
                    Amount = amount
                });
            }
        }

        var totalPremium = basePremium + adjustments.Sum(x => x.Amount);
        if (totalPremium < 0)
        {
            totalPremium = 0;
        }

        var quoteReference = $"Q-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8]}";
        var validUntil = DateTime.UtcNow.AddDays(7);

        dbContext.QuoteRecords.Add(new QuoteRecord
        {
            QuoteReference = quoteReference,
            ProductCode = product.ProductCode,
            CoverageAmount = request.CoverageAmount,
            ApplicantAge = request.ApplicantAge,
            VehicleValue = request.VehicleValue,
            SumAssured = request.SumAssured,
            BasePremium = basePremium,
            TotalPremium = decimal.Round(totalPremium, 2, MidpointRounding.AwayFromZero),
            CurrencyCode = product.CurrencyCode,
            SelectedRiderCodes = selectedRidersSerialized,
            ValidUntilUtc = validUntil,
            Status = "Quoted"
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return new GenerateQuoteResponse
        {
            QuoteReference = quoteReference,
            ProductCode = product.ProductCode,
            BasePremium = basePremium,
            TotalPremium = decimal.Round(totalPremium, 2, MidpointRounding.AwayFromZero),
            CurrencyCode = product.CurrencyCode,
            ValidUntilUtc = validUntil,
            Adjustments = adjustments,
            AppliedRiders = riderAdjustments
        };
    }

    private Task<ProductDefinition?> GetActiveProductDefinitionAsync(string productCode, CancellationToken cancellationToken)
    {
        var version = ProductCacheVersion.Get(memoryCache);
        var cacheKey = $"quote:product:v{version}:{productCode}";
        return memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            entry.SlidingExpiration = TimeSpan.FromMinutes(2);

            return await dbContext.ProductDefinitions
                .AsNoTracking()
                .Include(x => x.RiskRules)
                .Include(x => x.Riders)
                .SingleOrDefaultAsync(x => x.ProductCode == productCode && x.IsActive, cancellationToken);
        });
    }

    private static QuoteRecordResponse MapQuote(QuoteRecord quote)
    {
        return new QuoteRecordResponse
        {
            QuoteReference = quote.QuoteReference,
            ProductCode = quote.ProductCode,
            CoverageAmount = quote.CoverageAmount,
            ApplicantAge = quote.ApplicantAge,
            VehicleValue = quote.VehicleValue,
            SumAssured = quote.SumAssured,
            BasePremium = quote.BasePremium,
            TotalPremium = quote.TotalPremium,
            CurrencyCode = quote.CurrencyCode,
            ValidUntilUtc = quote.ValidUntilUtc,
            Status = quote.Status,
            IssuedPolicyNumber = quote.IssuedPolicyNumber,
            CreatedAtUtc = quote.CreatedAtUtc
        };
    }

    private static decimal ResolveParameter(string parameterName, GenerateQuoteRequest request)
    {
        return parameterName.ToLowerInvariant() switch
        {
            "applicantage" => request.ApplicantAge,
            "coverageamount" => request.CoverageAmount,
            "vehiclevalue" => request.VehicleValue,
            "sumassured" => request.SumAssured,
            _ => 0m
        };
    }

    private static bool Matches(string op, decimal input, decimal threshold)
    {
        return op switch
        {
            ">" => input > threshold,
            ">=" => input >= threshold,
            "<" => input < threshold,
            "<=" => input <= threshold,
            "==" => input == threshold,
            "!=" => input != threshold,
            _ => false
        };
    }
}
