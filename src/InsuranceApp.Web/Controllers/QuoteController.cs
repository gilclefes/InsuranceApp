using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Quotes;
using InsuranceApp.Infrastructure.Persistence;
using InsuranceApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Web.Controllers;

[AllowAnonymous]
public class QuoteController(InsuranceDbContext dbContext, IQuoteService quoteService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Start(string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return RedirectToAction("Index", "Products");
        }

        var product = await dbContext.ProductDefinitions
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.ProductCode == code && x.IsActive, cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        var riders = await dbContext.ProductRiders
            .AsNoTracking()
            .Where(x => x.ProductDefinitionId == product.Id && x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var model = new GetQuoteViewModel
        {
            ProductCode = product.ProductCode,
            ProductName = product.Name,
            AvailableRiders = riders.Select(r => new RiderOption
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
            return View(model);
        }
    }

    [HttpGet]
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
}
