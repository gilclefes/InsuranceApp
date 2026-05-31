using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Quotes;
using InsuranceApp.Infrastructure.Caching;
using InsuranceApp.Infrastructure.Persistence;
using InsuranceApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace InsuranceApp.Web.Controllers;

[AllowAnonymous]
public class QuoteController(InsuranceDbContext dbContext, IQuoteService quoteService, IMemoryCache memoryCache) : Controller
{
    [HttpGet]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Start(string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return RedirectToAction("Index", "Products");
        }

        var normalizedCode = code.Trim().ToUpperInvariant();
        var version = ProductCacheVersion.Get(memoryCache);
        var cacheKey = $"quote:start:v{version}:{normalizedCode}";

        var product = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            entry.SlidingExpiration = TimeSpan.FromMinutes(2);

            return await dbContext.ProductDefinitions
                .AsNoTracking()
                .Include(x => x.Riders.Where(r => r.IsActive))
                .SingleOrDefaultAsync(x => x.ProductCode == normalizedCode && x.IsActive, cancellationToken);
        });

        if (product is null)
        {
            return NotFound();
        }

        var model = new GetQuoteViewModel
        {
            ProductCode = product.ProductCode,
            ProductName = product.Name,
            ProductTypeId = (int)product.ProductType,
            ProductDescription = product.Description,
            MaxCoverageAmount = product.MaxCoverageAmount,
            AvailableRiders = product.Riders
            .OrderBy(x => x.Name)
            .Select(r => new RiderOption
            {
                RiderCode = r.RiderCode,
                Name = r.Name,
                AdjustmentType = r.AdjustmentType,
                AdjustmentValue = r.AdjustmentValue
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(GetQuoteViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            // Reload product metadata for the re-rendered form
            await ReloadProductMetadataAsync(model, cancellationToken);
            return View(model);
        }

        try
        {
            var quote = await quoteService.GenerateQuoteAsync(new GenerateQuoteRequest
            {
                ProductCode = model.ProductCode,
                CoverageAmount = model.CoverageAmount,
                ApplicantAge = model.ApplicantAge,
                VehicleValue = model.VehicleValue,
                SumAssured = model.SumAssured,
                SelectedRiderCodes = model.SelectedRiderCodes ?? new List<string>()
            }, cancellationToken);

            return RedirectToAction(nameof(Result), new { reference = quote.QuoteReference });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await ReloadProductMetadataAsync(model, cancellationToken);
            return View(model);
        }
    }

    [HttpGet]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Result(string reference, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            return RedirectToAction("Index", "Products");
        }

        var quote = await dbContext.QuoteRecords
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.QuoteReference == reference, cancellationToken);

        if (quote is null)
        {
            return NotFound();
        }

        var product = await dbContext.ProductDefinitions
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.ProductCode == quote.ProductCode, cancellationToken);

        return View(new QuoteResultViewModel
        {
            ProductName = product?.Name ?? quote.ProductCode,
            Quote = new GenerateQuoteResponse
            {
                QuoteReference = quote.QuoteReference,
                ProductCode = quote.ProductCode,
                BasePremium = quote.BasePremium,
                TotalPremium = quote.TotalPremium,
                CurrencyCode = quote.CurrencyCode,
                ValidUntilUtc = quote.ValidUntilUtc,
                Adjustments = Array.Empty<QuoteAdjustment>(),
                AppliedRiders = Array.Empty<QuoteRiderAdjustment>()
            }
        });
    }

    private async Task ReloadProductMetadataAsync(GetQuoteViewModel model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.ProductCode))
        {
            return;
        }

        var normalizedCode = model.ProductCode.Trim().ToUpperInvariant();
        var version = ProductCacheVersion.Get(memoryCache);
        var cacheKey = $"quote:start:v{version}:{normalizedCode}";
        var product = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            entry.SlidingExpiration = TimeSpan.FromMinutes(2);

            return await dbContext.ProductDefinitions
                .AsNoTracking()
                .Include(x => x.Riders.Where(r => r.IsActive))
                .SingleOrDefaultAsync(x => x.ProductCode == normalizedCode && x.IsActive, cancellationToken);
        });

        if (product is not null)
        {
            model.ProductTypeId = (int)product.ProductType;
            model.ProductDescription = product.Description;
            model.MaxCoverageAmount = product.MaxCoverageAmount;
        }

        if (model.AvailableRiders.Count == 0 && product is not null)
        {
            model.AvailableRiders = product.Riders
                .OrderBy(x => x.Name)
                .Select(r => new RiderOption
                {
                    RiderCode = r.RiderCode,
                    Name = r.Name,
                    AdjustmentType = r.AdjustmentType,
                    AdjustmentValue = r.AdjustmentValue
                })
                .ToList();
        }
    }
}
