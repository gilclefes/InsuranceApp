using InsuranceApp.Api.Controllers;
using InsuranceApp.Contracts.Products;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Tests.ApiContracts;

public class ProductAdminRiskRulesContractTests
{
    [Fact]
    public async Task CreateRiskRule_ShouldReturnOk_WhenProductExists()
    {
        await using var context = CreateDbContext(nameof(CreateRiskRule_ShouldReturnOk_WhenProductExists));
        context.ProductDefinitions.Add(new ProductDefinition
        {
            Id = 100,
            ProductCode = "MOTOR-TP-X",
            Name = "Motor TP X",
            ProductType = ProductType.Motor,
            CurrencyCode = "GHS",
            BaseRate = 0.01m,
            MinPremium = 100,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var controller = new ProductAdminController(context);
        var result = await controller.CreateRiskRule(100, new CreateProductRiskRuleRequest
        {
            ParameterName = "ApplicantAge",
            Operator = "<",
            ThresholdValue = 25,
            AdjustmentType = "Percent",
            AdjustmentValue = 12,
            Reason = "Young driver loading"
        }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<ProductRiskRuleResponse>(ok.Value);
        Assert.Equal("ApplicantAge", payload.ParameterName);
        Assert.Equal(1, await context.ProductRiskRules.CountAsync());
    }

    [Fact]
    public async Task UpdateRiskRule_ShouldReturnNotFound_WhenRuleMissing()
    {
        await using var context = CreateDbContext(nameof(UpdateRiskRule_ShouldReturnNotFound_WhenRuleMissing));
        var controller = new ProductAdminController(context);

        var result = await controller.UpdateRiskRule(1, 999, new UpdateProductRiskRuleRequest
        {
            ParameterName = "VehicleValue",
            Operator = ">",
            ThresholdValue = 100000,
            AdjustmentType = "Percent",
            AdjustmentValue = 5,
            Reason = "High value"
        }, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteRiskRule_ShouldDeleteRule_WhenExists()
    {
        await using var context = CreateDbContext(nameof(DeleteRiskRule_ShouldDeleteRule_WhenExists));
        context.ProductDefinitions.Add(new ProductDefinition
        {
            Id = 200,
            ProductCode = "FUNERAL-X",
            Name = "Funeral X",
            ProductType = ProductType.Funeral,
            CurrencyCode = "GHS",
            BaseRate = 0.02m,
            MinPremium = 50,
            IsActive = true
        });
        context.ProductRiskRules.Add(new ProductRiskRule
        {
            Id = 300,
            ProductDefinitionId = 200,
            ParameterName = "SumAssured",
            Operator = ">",
            ThresholdValue = 15000,
            AdjustmentType = "Flat",
            AdjustmentValue = 20,
            Reason = "Higher cover"
        });
        await context.SaveChangesAsync();

        var controller = new ProductAdminController(context);
        var result = await controller.DeleteRiskRule(200, 300, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(0, await context.ProductRiskRules.CountAsync());
    }

    [Fact]
    public async Task SearchRiskRules_ShouldFilterByParameterName()
    {
        await using var context = CreateDbContext(nameof(SearchRiskRules_ShouldFilterByParameterName));
        context.ProductDefinitions.Add(new ProductDefinition
        {
            Id = 400,
            ProductCode = "MOTOR-Z",
            Name = "Motor Z",
            ProductType = ProductType.Motor,
            CurrencyCode = "GHS",
            BaseRate = 0.01m,
            MinPremium = 90,
            IsActive = true
        });
        context.ProductRiskRules.AddRange(
            new ProductRiskRule
            {
                ProductDefinitionId = 400,
                ParameterName = "ApplicantAge",
                Operator = "<",
                ThresholdValue = 25,
                AdjustmentType = "Percent",
                AdjustmentValue = 10,
                Reason = "Young driver"
            },
            new ProductRiskRule
            {
                ProductDefinitionId = 400,
                ParameterName = "VehicleValue",
                Operator = ">",
                ThresholdValue = 120000,
                AdjustmentType = "Percent",
                AdjustmentValue = 7,
                Reason = "High value"
            });
        await context.SaveChangesAsync();

        var controller = new ProductAdminController(context);
        var result = await controller.SearchRiskRules(400, "ApplicantAge", string.Empty, string.Empty, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsAssignableFrom<IEnumerable<ProductRiskRuleResponse>>(ok.Value);
        var items = payload.ToList();
        Assert.Single(items);
        Assert.Equal("ApplicantAge", items[0].ParameterName);
    }

    [Fact]
    public async Task BulkUpsertRiskRules_ShouldCreateAndUpdateRules()
    {
        await using var context = CreateDbContext(nameof(BulkUpsertRiskRules_ShouldCreateAndUpdateRules));
        context.ProductDefinitions.Add(new ProductDefinition
        {
            Id = 500,
            ProductCode = "FUNERAL-Z",
            Name = "Funeral Z",
            ProductType = ProductType.Funeral,
            CurrencyCode = "GHS",
            BaseRate = 0.02m,
            MinPremium = 60,
            IsActive = true
        });
        context.ProductRiskRules.Add(new ProductRiskRule
        {
            Id = 501,
            ProductDefinitionId = 500,
            ParameterName = "SumAssured",
            Operator = ">",
            ThresholdValue = 10000,
            AdjustmentType = "Flat",
            AdjustmentValue = 10,
            Reason = "Initial reason"
        });
        await context.SaveChangesAsync();

        var controller = new ProductAdminController(context);
        var result = await controller.BulkUpsertRiskRules(new BulkUpsertProductRiskRulesRequest
        {
            ProductDefinitionId = 500,
            Rules = new List<BulkUpsertProductRiskRuleItem>
            {
                new()
                {
                    Id = 501,
                    ParameterName = "SumAssured",
                    Operator = ">",
                    ThresholdValue = 12000,
                    AdjustmentType = "Flat",
                    AdjustmentValue = 12,
                    Reason = "Updated reason"
                },
                new()
                {
                    ParameterName = "ApplicantAge",
                    Operator = "<",
                    ThresholdValue = 30,
                    AdjustmentType = "Percent",
                    AdjustmentValue = 5,
                    Reason = "Age uplift"
                }
            }
        }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<BulkUpsertProductRiskRulesResponse>(ok.Value);
        Assert.Equal(2, payload.TotalProcessed);
        Assert.Equal(1, payload.CreatedCount);
        Assert.Equal(1, payload.UpdatedCount);
        Assert.Equal(2, await context.ProductRiskRules.CountAsync(x => x.ProductDefinitionId == 500));
    }

    private static InsuranceDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<InsuranceDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new InsuranceDbContext(options);
    }
}
