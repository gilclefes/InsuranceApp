using InsuranceApp.Contracts.PremiumCollections;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Payments;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Tests.Payments;

public class PremiumCollectionServiceTests
{
    [Fact]
    public async Task ScheduleCollectionAsync_ShouldCreatePendingTransaction()
    {
        await using var context = CreateDbContext(nameof(ScheduleCollectionAsync_ShouldCreatePendingTransaction));
        context.Policies.Add(new Policy
        {
            PolicyNumber = "POL-S4-1",
            CustomerId = 1,
            ProductType = ProductType.Motor,
            CoverageType = "Standard",
            PremiumAmount = 200,
            CurrencyCode = "GHS",
            InceptionDate = DateTime.UtcNow.Date,
            ExpiryDate = DateTime.UtcNow.Date.AddYears(1),
            Status = PolicyStatus.Active
        });
        await context.SaveChangesAsync();

        var service = new PremiumCollectionService(context);
        var response = await service.ScheduleCollectionAsync("POL-S4-1", new SchedulePremiumCollectionRequest
        {
            DueDateUtc = DateTime.UtcNow.Date.AddDays(1),
            Amount = 250
        });

        Assert.Equal("Pending", response.Status);
        Assert.Equal(250, response.Amount);
        Assert.Equal(1, await context.PremiumTransactions.CountAsync());
    }

    [Fact]
    public async Task RunDueCollectionsAsync_ShouldProcessDueTransactions()
    {
        await using var context = CreateDbContext(nameof(RunDueCollectionsAsync_ShouldProcessDueTransactions));
        context.Policies.Add(new Policy
        {
            PolicyNumber = "POL-S4-2",
            CustomerId = 1,
            ProductType = ProductType.Motor,
            CoverageType = "Standard",
            PremiumAmount = 200,
            CurrencyCode = "GHS",
            InceptionDate = DateTime.UtcNow.Date,
            ExpiryDate = DateTime.UtcNow.Date.AddYears(1),
            Status = PolicyStatus.Active
        });
        await context.SaveChangesAsync();

        var policy = await context.Policies.SingleAsync();
        context.PremiumTransactions.Add(new PremiumTransaction
        {
            PolicyId = policy.Id,
            TransactionReference = "COL-TEST-1",
            Provider = "MTN_MOMO",
            PaymentChannel = "MoMo",
            Amount = 200,
            Status = PaymentStatus.Pending,
            DueDateUtc = DateTime.UtcNow.Date.AddDays(-1)
        });
        await context.SaveChangesAsync();

        var service = new PremiumCollectionService(context);
        var result = await service.RunDueCollectionsAsync();

        Assert.Equal(1, result.ItemsScanned);
        Assert.Equal(1, result.ItemsSucceeded);

        var transaction = await context.PremiumTransactions.SingleAsync();
        Assert.Equal(PaymentStatus.Success, transaction.Status);
    }

    private static InsuranceDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<InsuranceDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var context = new InsuranceDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
