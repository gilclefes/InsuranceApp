using InsuranceApp.Contracts.Products;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Caching;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace InsuranceApp.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/admin/products")]
public class ProductAdminController(InsuranceDbContext dbContext, IMemoryCache? cache = null) : ControllerBase
{
    private readonly IMemoryCache memoryCache = cache ?? new MemoryCache(new MemoryCacheOptions());

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var items = await dbContext.ProductDefinitions
            .AsNoTracking()
            .OrderBy(x => x.ProductCode)
            .Select(x => new ProductDefinitionResponse
            {
                Id = x.Id,
                ProductCode = x.ProductCode,
                Name = x.Name,
                Description = x.Description,
                CoverageSummary = x.CoverageSummary,
                ProductType = (int)x.ProductType,
                NicClassCode = x.NicClassCode,
                CurrencyCode = x.CurrencyCode,
                BaseRate = x.BaseRate,
                MinPremium = x.MinPremium,
                MaxCoverageAmount = x.MaxCoverageAmount,
                MinEntryAgeYears = x.MinEntryAgeYears,
                MaxEntryAgeYears = x.MaxEntryAgeYears,
                PolicyTermOptions = x.PolicyTermOptions,
                WaitingPeriodDays = x.WaitingPeriodDays,
                Exclusions = x.Exclusions,
                UnderwritingRequirements = x.UnderwritingRequirements,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDefinitionRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var exists = await dbContext.ProductDefinitions.AnyAsync(x => x.ProductCode == request.ProductCode, cancellationToken);
        if (exists)
        {
            return BadRequest(new { message = "Product code already exists." });
        }

        var entity = new ProductDefinition
        {
            ProductCode = request.ProductCode.Trim().ToUpperInvariant(),
            Name = request.Name,
            Description = request.Description,
            CoverageSummary = request.CoverageSummary,
            ProductType = Enum.IsDefined(typeof(ProductType), request.ProductType) ? (ProductType)request.ProductType : ProductType.Custom,
            NicClassCode = request.NicClassCode,
            CurrencyCode = request.CurrencyCode,
            BaseRate = request.BaseRate,
            MinPremium = request.MinPremium,
            MaxCoverageAmount = request.MaxCoverageAmount,
            MinEntryAgeYears = request.MinEntryAgeYears,
            MaxEntryAgeYears = request.MaxEntryAgeYears,
            PolicyTermOptions = request.PolicyTermOptions,
            WaitingPeriodDays = request.WaitingPeriodDays,
            Exclusions = request.Exclusions,
            UnderwritingRequirements = request.UnderwritingRequirements,
            IsActive = true
        };

        dbContext.ProductDefinitions.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        ProductCacheVersion.Bump(memoryCache);

        return Ok(MapToResponse(entity));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateProductDefinitionRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var entity = await dbContext.ProductDefinitions.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return NotFound(new { message = "Product not found." });
        }

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.CoverageSummary = request.CoverageSummary;
        entity.NicClassCode = request.NicClassCode;
        entity.CurrencyCode = request.CurrencyCode;
        entity.BaseRate = request.BaseRate;
        entity.MinPremium = request.MinPremium;
        entity.MaxCoverageAmount = request.MaxCoverageAmount;
        entity.MinEntryAgeYears = request.MinEntryAgeYears;
        entity.MaxEntryAgeYears = request.MaxEntryAgeYears;
        entity.PolicyTermOptions = request.PolicyTermOptions;
        entity.WaitingPeriodDays = request.WaitingPeriodDays;
        entity.Exclusions = request.Exclusions;
        entity.UnderwritingRequirements = request.UnderwritingRequirements;
        entity.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);
        ProductCacheVersion.Bump(memoryCache);

        return Ok(MapToResponse(entity));
    }

    private static ProductDefinitionResponse MapToResponse(ProductDefinition e) => new()
    {
        Id = e.Id,
        ProductCode = e.ProductCode,
        Name = e.Name,
        Description = e.Description,
        CoverageSummary = e.CoverageSummary,
        ProductType = (int)e.ProductType,
        NicClassCode = e.NicClassCode,
        CurrencyCode = e.CurrencyCode,
        BaseRate = e.BaseRate,
        MinPremium = e.MinPremium,
        MaxCoverageAmount = e.MaxCoverageAmount,
        MinEntryAgeYears = e.MinEntryAgeYears,
        MaxEntryAgeYears = e.MaxEntryAgeYears,
        PolicyTermOptions = e.PolicyTermOptions,
        WaitingPeriodDays = e.WaitingPeriodDays,
        Exclusions = e.Exclusions,
        UnderwritingRequirements = e.UnderwritingRequirements,
        IsActive = e.IsActive
    };

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Deactivate(long id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ProductDefinitions.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return NotFound(new { message = "Product not found." });
        }

        entity.IsActive = false;
        await dbContext.SaveChangesAsync(cancellationToken);
        ProductCacheVersion.Bump(memoryCache);

        return Ok(new { message = "Product deactivated." });
    }

    [HttpGet("{id:long}/risk-rules")]
    public async Task<IActionResult> ListRiskRules(long id, CancellationToken cancellationToken)
    {
        var productExists = await dbContext.ProductDefinitions.AnyAsync(x => x.Id == id, cancellationToken);
        if (!productExists)
        {
            return NotFound(new { message = "Product not found." });
        }

        var items = await dbContext.ProductRiskRules
            .AsNoTracking()
            .Where(x => x.ProductDefinitionId == id)
            .OrderBy(x => x.ParameterName)
            .ThenBy(x => x.Id)
            .Select(x => new ProductRiskRuleResponse
            {
                Id = x.Id,
                ProductDefinitionId = x.ProductDefinitionId,
                ParameterName = x.ParameterName,
                Operator = x.Operator,
                ThresholdValue = x.ThresholdValue,
                AdjustmentType = x.AdjustmentType,
                AdjustmentValue = x.AdjustmentValue,
                Reason = x.Reason
            })
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpGet("risk-rules")]
    public async Task<IActionResult> SearchRiskRules(
        [FromQuery] long? productDefinitionId,
        [FromQuery] string parameterName,
        [FromQuery] string adjustmentType,
        [FromQuery] string @operator,
        CancellationToken cancellationToken)
    {
        var query = dbContext.ProductRiskRules.AsNoTracking().AsQueryable();

        if (productDefinitionId.HasValue)
        {
            query = query.Where(x => x.ProductDefinitionId == productDefinitionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameterName))
        {
            query = query.Where(x => x.ParameterName == parameterName);
        }

        if (!string.IsNullOrWhiteSpace(adjustmentType))
        {
            query = query.Where(x => x.AdjustmentType == adjustmentType);
        }

        if (!string.IsNullOrWhiteSpace(@operator))
        {
            query = query.Where(x => x.Operator == @operator);
        }

        var items = await query
            .OrderBy(x => x.ProductDefinitionId)
            .ThenBy(x => x.ParameterName)
            .ThenBy(x => x.Id)
            .Select(x => new ProductRiskRuleResponse
            {
                Id = x.Id,
                ProductDefinitionId = x.ProductDefinitionId,
                ParameterName = x.ParameterName,
                Operator = x.Operator,
                ThresholdValue = x.ThresholdValue,
                AdjustmentType = x.AdjustmentType,
                AdjustmentValue = x.AdjustmentValue,
                Reason = x.Reason
            })
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpPost("{id:long}/risk-rules")]
    public async Task<IActionResult> CreateRiskRule(long id, [FromBody] CreateProductRiskRuleRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var productExists = await dbContext.ProductDefinitions.AnyAsync(x => x.Id == id, cancellationToken);
        if (!productExists)
        {
            return NotFound(new { message = "Product not found." });
        }

        var entity = new ProductRiskRule
        {
            ProductDefinitionId = id,
            ParameterName = request.ParameterName,
            Operator = request.Operator,
            ThresholdValue = request.ThresholdValue,
            AdjustmentType = request.AdjustmentType,
            AdjustmentValue = request.AdjustmentValue,
            Reason = request.Reason
        };

        dbContext.ProductRiskRules.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        ProductCacheVersion.Bump(memoryCache);

        return Ok(new ProductRiskRuleResponse
        {
            Id = entity.Id,
            ProductDefinitionId = entity.ProductDefinitionId,
            ParameterName = entity.ParameterName,
            Operator = entity.Operator,
            ThresholdValue = entity.ThresholdValue,
            AdjustmentType = entity.AdjustmentType,
            AdjustmentValue = entity.AdjustmentValue,
            Reason = entity.Reason
        });
    }

    [HttpPut("{id:long}/risk-rules/{ruleId:long}")]
    public async Task<IActionResult> UpdateRiskRule(long id, long ruleId, [FromBody] UpdateProductRiskRuleRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var entity = await dbContext.ProductRiskRules
            .SingleOrDefaultAsync(x => x.Id == ruleId && x.ProductDefinitionId == id, cancellationToken);

        if (entity is null)
        {
            return NotFound(new { message = "Risk rule not found." });
        }

        entity.ParameterName = request.ParameterName;
        entity.Operator = request.Operator;
        entity.ThresholdValue = request.ThresholdValue;
        entity.AdjustmentType = request.AdjustmentType;
        entity.AdjustmentValue = request.AdjustmentValue;
        entity.Reason = request.Reason;

        await dbContext.SaveChangesAsync(cancellationToken);
        ProductCacheVersion.Bump(memoryCache);

        return Ok(new ProductRiskRuleResponse
        {
            Id = entity.Id,
            ProductDefinitionId = entity.ProductDefinitionId,
            ParameterName = entity.ParameterName,
            Operator = entity.Operator,
            ThresholdValue = entity.ThresholdValue,
            AdjustmentType = entity.AdjustmentType,
            AdjustmentValue = entity.AdjustmentValue,
            Reason = entity.Reason
        });
    }

    [HttpDelete("{id:long}/risk-rules/{ruleId:long}")]
    public async Task<IActionResult> DeleteRiskRule(long id, long ruleId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ProductRiskRules
            .SingleOrDefaultAsync(x => x.Id == ruleId && x.ProductDefinitionId == id, cancellationToken);

        if (entity is null)
        {
            return NotFound(new { message = "Risk rule not found." });
        }

        dbContext.ProductRiskRules.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        ProductCacheVersion.Bump(memoryCache);

        return Ok(new { message = "Risk rule deleted." });
    }

    [HttpGet("{id:long}/riders")]
    public async Task<IActionResult> ListRiders(long id, CancellationToken cancellationToken)
    {
        var productExists = await dbContext.ProductDefinitions.AnyAsync(x => x.Id == id, cancellationToken);
        if (!productExists)
        {
            return NotFound(new { message = "Product not found." });
        }

        var items = await dbContext.ProductRiders
            .AsNoTracking()
            .Where(x => x.ProductDefinitionId == id)
            .OrderBy(x => x.RiderCode)
            .Select(x => new ProductRiderResponse
            {
                Id = x.Id,
                ProductDefinitionId = x.ProductDefinitionId,
                RiderCode = x.RiderCode,
                Name = x.Name,
                AdjustmentType = x.AdjustmentType,
                AdjustmentValue = x.AdjustmentValue,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpPost("{id:long}/riders")]
    public async Task<IActionResult> CreateRider(long id, [FromBody] CreateProductRiderRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var productExists = await dbContext.ProductDefinitions.AnyAsync(x => x.Id == id, cancellationToken);
        if (!productExists)
        {
            return NotFound(new { message = "Product not found." });
        }

        var riderCode = request.RiderCode.Trim().ToUpperInvariant();
        var exists = await dbContext.ProductRiders.AnyAsync(x => x.ProductDefinitionId == id && x.RiderCode == riderCode, cancellationToken);
        if (exists)
        {
            return BadRequest(new { message = "Rider code already exists for this product." });
        }

        var entity = new ProductRider
        {
            ProductDefinitionId = id,
            RiderCode = riderCode,
            Name = request.Name,
            AdjustmentType = request.AdjustmentType,
            AdjustmentValue = request.AdjustmentValue,
            IsActive = true
        };

        dbContext.ProductRiders.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        ProductCacheVersion.Bump(memoryCache);

        return Ok(new ProductRiderResponse
        {
            Id = entity.Id,
            ProductDefinitionId = entity.ProductDefinitionId,
            RiderCode = entity.RiderCode,
            Name = entity.Name,
            AdjustmentType = entity.AdjustmentType,
            AdjustmentValue = entity.AdjustmentValue,
            IsActive = entity.IsActive
        });
    }

    [HttpPut("{id:long}/riders/{riderId:long}")]
    public async Task<IActionResult> UpdateRider(long id, long riderId, [FromBody] UpdateProductRiderRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var entity = await dbContext.ProductRiders
            .SingleOrDefaultAsync(x => x.Id == riderId && x.ProductDefinitionId == id, cancellationToken);

        if (entity is null)
        {
            return NotFound(new { message = "Rider not found." });
        }

        entity.Name = request.Name;
        entity.AdjustmentType = request.AdjustmentType;
        entity.AdjustmentValue = request.AdjustmentValue;
        entity.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);
        ProductCacheVersion.Bump(memoryCache);

        return Ok(new ProductRiderResponse
        {
            Id = entity.Id,
            ProductDefinitionId = entity.ProductDefinitionId,
            RiderCode = entity.RiderCode,
            Name = entity.Name,
            AdjustmentType = entity.AdjustmentType,
            AdjustmentValue = entity.AdjustmentValue,
            IsActive = entity.IsActive
        });
    }

    [HttpDelete("{id:long}/riders/{riderId:long}")]
    public async Task<IActionResult> DeactivateRider(long id, long riderId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.ProductRiders
            .SingleOrDefaultAsync(x => x.Id == riderId && x.ProductDefinitionId == id, cancellationToken);

        if (entity is null)
        {
            return NotFound(new { message = "Rider not found." });
        }

        entity.IsActive = false;
        await dbContext.SaveChangesAsync(cancellationToken);
        ProductCacheVersion.Bump(memoryCache);

        return Ok(new { message = "Rider deactivated." });
    }

    [HttpPost("risk-rules/bulk-upsert")]
    public async Task<IActionResult> BulkUpsertRiskRules([FromBody] BulkUpsertProductRiskRulesRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var productExists = await dbContext.ProductDefinitions.AnyAsync(x => x.Id == request.ProductDefinitionId, cancellationToken);
        if (!productExists)
        {
            return NotFound(new { message = "Product not found." });
        }

        if (request.Rules.Count == 0)
        {
            return BadRequest(new { message = "At least one risk rule is required." });
        }

        var requestedIds = request.Rules.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToHashSet();
        var existing = await dbContext.ProductRiskRules
            .Where(x => x.ProductDefinitionId == request.ProductDefinitionId && requestedIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var createdCount = 0;
        var updatedCount = 0;
        var touched = new List<ProductRiskRule>();

        foreach (var item in request.Rules)
        {
            if (item.Id.HasValue && existing.TryGetValue(item.Id.Value, out var existingRule))
            {
                existingRule.ParameterName = item.ParameterName;
                existingRule.Operator = item.Operator;
                existingRule.ThresholdValue = item.ThresholdValue;
                existingRule.AdjustmentType = item.AdjustmentType;
                existingRule.AdjustmentValue = item.AdjustmentValue;
                existingRule.Reason = item.Reason;

                updatedCount++;
                touched.Add(existingRule);
                continue;
            }

            var created = new ProductRiskRule
            {
                ProductDefinitionId = request.ProductDefinitionId,
                ParameterName = item.ParameterName,
                Operator = item.Operator,
                ThresholdValue = item.ThresholdValue,
                AdjustmentType = item.AdjustmentType,
                AdjustmentValue = item.AdjustmentValue,
                Reason = item.Reason
            };

            dbContext.ProductRiskRules.Add(created);
            createdCount++;
            touched.Add(created);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        ProductCacheVersion.Bump(memoryCache);

        return Ok(new BulkUpsertProductRiskRulesResponse
        {
            TotalProcessed = request.Rules.Count,
            CreatedCount = createdCount,
            UpdatedCount = updatedCount,
            Items = touched
                .Select(x => new ProductRiskRuleResponse
                {
                    Id = x.Id,
                    ProductDefinitionId = x.ProductDefinitionId,
                    ParameterName = x.ParameterName,
                    Operator = x.Operator,
                    ThresholdValue = x.ThresholdValue,
                    AdjustmentType = x.AdjustmentType,
                    AdjustmentValue = x.AdjustmentValue,
                    Reason = x.Reason
                })
                .ToList()
        });
    }
}
