using InsuranceApp.Domain.Entities;
using InsuranceApp.Infrastructure.Persistence;
using InsuranceApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Web.Controllers;

[Authorize(Roles = "Admin")]
public class ProductAdminController(InsuranceDbContext dbContext) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var products = await dbContext.ProductDefinitions
            .OrderBy(x => x.ProductCode)
            .ToListAsync(cancellationToken);

        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Configure(long id, CancellationToken cancellationToken)
    {
        var product = await dbContext.ProductDefinitions
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        var rules = await dbContext.ProductRiskRules
            .Where(x => x.ProductDefinitionId == id)
            .OrderBy(x => x.ParameterName)
            .ToListAsync(cancellationToken);

        var riders = await dbContext.ProductRiders
            .Where(x => x.ProductDefinitionId == id)
            .OrderBy(x => x.RiderCode)
            .ToListAsync(cancellationToken);

        return View(new ProductConfigViewModel
        {
            Product = product,
            RiskRules = rules,
            Riders = riders
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRider(long productId, string riderCode, string name, string adjustmentType, decimal adjustmentValue, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(riderCode) || string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Rider code and name are required.";
            return RedirectToAction(nameof(Configure), new { id = productId });
        }

        var code = riderCode.Trim().ToUpperInvariant();
        var exists = await dbContext.ProductRiders.AnyAsync(x => x.ProductDefinitionId == productId && x.RiderCode == code, cancellationToken);
        if (exists)
        {
            TempData["Error"] = "Rider code already exists for this product.";
            return RedirectToAction(nameof(Configure), new { id = productId });
        }

        dbContext.ProductRiders.Add(new ProductRider
        {
            ProductDefinitionId = productId,
            RiderCode = code,
            Name = name.Trim(),
            AdjustmentType = string.IsNullOrWhiteSpace(adjustmentType) ? "Flat" : adjustmentType.Trim(),
            AdjustmentValue = adjustmentValue,
            IsActive = true
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Rider added.";
        return RedirectToAction(nameof(Configure), new { id = productId });
    }
}
