using InsuranceApp.Api.Controllers;
using InsuranceApp.Contracts.Products;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Tests.ApiContracts;

public class ProductAdminRidersContractTests
{
    [Fact]
    public async Task CreateRider_ShouldReturnOk_WhenProductExists()
    {
        await using var context = CreateDbContext(nameof(CreateRider_ShouldReturnOk_WhenProductExists));
        context.ProductDefinitions.Add(new ProductDefinition
        {
            Id = 700,
            ProductCode = "MOTOR-R",
            Name = "Motor Rider",
            ProductType = ProductType.Motor,
            CurrencyCode = "GHS",
            BaseRate = 0.01m,
            MinPremium = 100,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var controller = new ProductAdminController(context);
        var result = await controller.CreateRider(700, new CreateProductRiderRequest
        {
            RiderCode = "WINDSHIELD",
            Name = "Windscreen",
            AdjustmentType = "Flat",
            AdjustmentValue = 20
        }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<ProductRiderResponse>(ok.Value);
        Assert.Equal("WINDSHIELD", payload.RiderCode);
    }

    [Fact]
    public async Task DeactivateRider_ShouldReturnOk_WhenFound()
    {
        await using var context = CreateDbContext(nameof(DeactivateRider_ShouldReturnOk_WhenFound));
        context.ProductDefinitions.Add(new ProductDefinition
        {
            Id = 701,
            ProductCode = "FUNERAL-R",
            Name = "Funeral Rider",
            ProductType = ProductType.Funeral,
            CurrencyCode = "GHS",
            BaseRate = 0.02m,
            MinPremium = 50,
            IsActive = true
        });
        context.ProductRiders.Add(new ProductRider
        {
            Id = 801,
            ProductDefinitionId = 701,
            RiderCode = "FAMILY_PLUS",
            Name = "Family Plus",
            AdjustmentType = "Percent",
            AdjustmentValue = 5,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var controller = new ProductAdminController(context);
        var result = await controller.DeactivateRider(701, 801, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        var rider = await context.ProductRiders.SingleAsync(x => x.Id == 801);
        Assert.False(rider.IsActive);
    }

    private static InsuranceDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<InsuranceDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new InsuranceDbContext(options);
    }
}
