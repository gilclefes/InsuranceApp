using InsuranceApp.Contracts.PremiumCollections;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Payments;
using InsuranceApp.Infrastructure.Payments.Providers;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

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

        var service = CreateService(context);
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

        var service = CreateService(context);
        var result = await service.RunDueCollectionsAsync();

        Assert.Equal(1, result.ItemsScanned);
        Assert.Equal(1, result.ItemsSucceeded);

        var transaction = await context.PremiumTransactions.SingleAsync();
        Assert.Equal(PaymentStatus.Success, transaction.Status);
    }

    [Fact]
    public async Task ProcessWebhookAsync_ShouldBeIdempotentByEventId()
    {
        await using var context = CreateDbContext(nameof(ProcessWebhookAsync_ShouldBeIdempotentByEventId));
        context.Policies.Add(new Policy
        {
            PolicyNumber = "POL-S4-3",
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
            TransactionReference = "COL-POL-S4-3-1",
            Provider = "MTN_MOMO",
            PaymentChannel = "MoMo",
            Amount = 200,
            Status = PaymentStatus.Pending,
            DueDateUtc = DateTime.UtcNow.Date
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var first = await service.ProcessWebhookAsync(new PremiumCollectionWebhookRequest
        {
            Provider = "MTN_MOMO",
            EventId = "EVT-001",
            TransactionReference = "COL-POL-S4-3-1",
            PolicyNumber = "POL-S4-3",
            Status = "success",
            Signature = "sig-1",
            EventTimeUtc = DateTime.UtcNow
        });

        var second = await service.ProcessWebhookAsync(new PremiumCollectionWebhookRequest
        {
            Provider = "MTN_MOMO",
            EventId = "EVT-001",
            TransactionReference = "COL-POL-S4-3-1",
            PolicyNumber = "POL-S4-3",
            Status = "success",
            Signature = "sig-1",
            EventTimeUtc = DateTime.UtcNow
        });

        Assert.True(first.Processed);
        Assert.False(first.IsDuplicate);
        Assert.True(second.IsDuplicate);
        Assert.Equal(1, await context.PremiumCollectionWebhookLogs.CountAsync());

        var transaction = await context.PremiumTransactions.SingleAsync();
        Assert.Equal(PaymentStatus.Success, transaction.Status);
    }

    [Fact]
    public async Task GetReconciliationSummaryAsync_ShouldReturnExceptionItems()
    {
        await using var context = CreateDbContext(nameof(GetReconciliationSummaryAsync_ShouldReturnExceptionItems));
        context.Policies.Add(new Policy
        {
            PolicyNumber = "POL-S4-4",
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
        context.PremiumTransactions.AddRange(
            new PremiumTransaction
            {
                PolicyId = policy.Id,
                TransactionReference = "COL-S4-OK",
                Provider = "MTN_MOMO",
                PaymentChannel = "MoMo",
                Amount = 200,
                Status = PaymentStatus.Success,
                DueDateUtc = DateTime.UtcNow.Date
            },
            new PremiumTransaction
            {
                PolicyId = policy.Id,
                TransactionReference = "COL-S4-ERR",
                Provider = "MTN_MOMO",
                PaymentChannel = "MoMo",
                Amount = 120,
                Status = PaymentStatus.Failed,
                DueDateUtc = DateTime.UtcNow.Date
            });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var summary = await service.GetReconciliationSummaryAsync(DateTime.UtcNow.Date.AddDays(-1), DateTime.UtcNow.Date.AddDays(1));

        Assert.Equal(2, summary.TotalTransactions);
        Assert.Equal(1, summary.SuccessCount);
        Assert.Equal(1, summary.FailedCount);
        Assert.Single(summary.Exceptions);
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

    private static PremiumCollectionService CreateService(InsuranceDbContext context)
    {
        var simulated = new SimulatedPaymentGateway(NullLogger<SimulatedPaymentGateway>.Instance);
        var router = new PaymentGatewayRouter(new[] { (Application.Interfaces.IPaymentGateway)simulated }, simulated);
        var validator = new HmacWebhookSignatureValidator(Options.Create(new PaymentGatewayOptions()));
        return new PremiumCollectionService(context, router, validator);
    }
}
