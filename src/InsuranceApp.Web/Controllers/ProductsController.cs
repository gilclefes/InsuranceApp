using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Persistence;
using InsuranceApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Web.Controllers;

[AllowAnonymous]
public class ProductsController(InsuranceDbContext dbContext) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? type, CancellationToken cancellationToken)
    {
        var query = dbContext.ProductDefinitions
            .AsNoTracking()
            .Where(x => x.IsActive);

        if (Enum.TryParse<ProductType>(type, true, out var parsed))
        {
            query = query.Where(x => x.ProductType == parsed);
        }

        var products = await query
            .OrderBy(x => x.ProductType)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return View(new ProductCatalogViewModel { Products = products, TypeFilter = type });
    }

    [HttpGet]
    public async Task<IActionResult> Details(string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return RedirectToAction(nameof(Index));
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

        return View(new ProductDetailsViewModel { Product = product, Riders = riders });
    }
}
