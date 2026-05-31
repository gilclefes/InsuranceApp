using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Caching;
using InsuranceApp.Infrastructure.Persistence;
using InsuranceApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace InsuranceApp.Web.Controllers;

[AllowAnonymous]
public class ProductsController(InsuranceDbContext dbContext, IMemoryCache memoryCache) : Controller
{
    [HttpGet]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Index(string? type, CancellationToken cancellationToken)
    {
        var normalizedType = (type ?? string.Empty).Trim().ToUpperInvariant();
        var version = ProductCacheVersion.Get(memoryCache);
        var cacheKey = $"products:index:v{version}:{normalizedType}";

        var products = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            entry.SlidingExpiration = TimeSpan.FromMinutes(2);

            var query = dbContext.ProductDefinitions
                .AsNoTracking()
                .Where(x => x.IsActive);

            if (Enum.TryParse<ProductType>(normalizedType, true, out var parsed))
            {
                query = query.Where(x => x.ProductType == parsed);
            }

            return await query
                .OrderBy(x => x.ProductType)
                .ThenBy(x => x.Name)
                .ToListAsync(cancellationToken);
        }) ?? new List<ProductDefinition>();

        return View(new ProductCatalogViewModel { Products = products, TypeFilter = type });
    }

    [HttpGet]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Details(string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return RedirectToAction(nameof(Index));
        }

        var normalizedCode = code.Trim().ToUpperInvariant();
        var version = ProductCacheVersion.Get(memoryCache);
        var cacheKey = $"products:details:v{version}:{normalizedCode}";

        var details = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            entry.SlidingExpiration = TimeSpan.FromMinutes(2);

            return await dbContext.ProductDefinitions
                .AsNoTracking()
                .Include(x => x.Riders.Where(r => r.IsActive))
                .SingleOrDefaultAsync(x => x.ProductCode == normalizedCode && x.IsActive, cancellationToken);
        });

        var product = details;

        if (product is null)
        {
            return NotFound();
        }

        var riders = product.Riders
            .OrderBy(x => x.Name)
            .ToList();

        return View(new ProductDetailsViewModel { Product = product, Riders = riders });
    }
}
