using InsuranceApp.Contracts.Payouts;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Persistence;
using InsuranceApp.Infrastructure.Payouts;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Tests.Payouts;

public class PayoutServiceTests
{
    [Fact]
    public async Task InitiatePayoutAsync_ShouldCreateInitiatedPayout()
    {
        await using var context = CreateDbContext(nameof(InitiatePayoutAsync_ShouldCreateInitiatedPayout));
        SeedApprovedClaim(context, "CLM-PO-1");
        await context.SaveChangesAsync();

        var service = new PayoutService(context);
        var payout = await service.InitiatePayoutAsync(new InitiatePayoutRequest
        {
            ClaimNumber = "CLM-PO-1",
            Amount = 200,
            DestinationChannel = "MoMo",
            DestinationAccount = "233240000000"
        });

        Assert.Equal("Initiated", payout.Status);
        Assert.Equal(1, await context.PayoutTransactions.CountAsync());
    }

    [Fact]
    public async Task RunReconciliationAsync_ShouldDisburseEvenAmount()
    {
        await using var context = CreateDbContext(nameof(RunReconciliationAsync_ShouldDisburseEvenAmount));
        SeedApprovedClaim(context, "CLM-PO-2");
        await context.SaveChangesAsync();

        var claim = await context.Claims.SingleAsync(x => x.ClaimNumber == "CLM-PO-2");
        context.PayoutTransactions.Add(new PayoutTransaction
        {
            ClaimId = claim.Id,
            PayoutReference = "PO-CLM-PO-2-1",
            IdempotencyKey = "IDEM-1",
            Amount = 200,
            Status = PaymentStatus.Initiated,
            DestinationChannel = "MoMo",
            DestinationAccount = "233240000000"
        });
        await context.SaveChangesAsync();

        var service = new PayoutService(context);
        var result = await service.RunReconciliationAsync();

        Assert.Equal(1, result.ItemsScanned);
        Assert.Equal(1, result.ReconciledCount);

        var payout = await context.PayoutTransactions.SingleAsync();
        Assert.Equal(PaymentStatus.Disbursed, payout.Status);
    }

    private static void SeedApprovedClaim(InsuranceDbContext context, string claimNumber)
    {
        context.Customers.Add(new Customer
        {
            Id = 901,
            CustomerNumber = "CUS-901",
            GhanaCardNumberHash = "hash",
            FirstName = "Nana",
            LastName = "Adjei",
            DateOfBirth = new DateTime(1990, 1, 1),
            PhoneNumber = "+233200000901",
            Email = "nana@example.com",
            ConsentAccepted = true,
            ConsentAcceptedAtUtc = DateTime.UtcNow
        });

        context.Policies.Add(new Policy
        {
            PolicyNumber = "POL-PO-1",
            CustomerId = 901,
            ProductType = ProductType.Motor,
            CoverageType = "Standard",
            PremiumAmount = 120,
            CurrencyCode = "GHS",
            InceptionDate = DateTime.UtcNow.Date.AddMonths(-6),
            ExpiryDate = DateTime.UtcNow.Date.AddMonths(6),
            Status = PolicyStatus.Active
        });

        context.Claims.Add(new Claim
        {
            ClaimNumber = claimNumber,
            PolicyId = 1,
            IncidentDate = DateTime.UtcNow.Date.AddDays(-10),
            ClaimType = "Accident",
            ClaimedAmount = 300,
            ApprovedAmount = 200,
            Status = ClaimStatus.Approved,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-5),
            UpdatedAtUtc = DateTime.UtcNow.AddDays(-1)
        });
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