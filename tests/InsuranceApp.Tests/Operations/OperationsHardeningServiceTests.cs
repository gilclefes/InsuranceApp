using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Privacy;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Operations;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Tests.Operations;

public class OperationsHardeningServiceTests
{
    [Fact]
    public async Task GetReadinessSummaryAsync_ShouldReturnOperationalSnapshot()
    {
        await using var context = CreateDbContext(nameof(GetReadinessSummaryAsync_ShouldReturnOperationalSnapshot));
        SeedOperationalData(context);
        await context.SaveChangesAsync();

        var service = new OperationsHardeningService(context, new StubRetentionService());
        var summary = await service.GetReadinessSummaryAsync();

        Assert.True(summary.DatabaseConnectivityHealthy);
        Assert.Equal(1, summary.PendingPremiumCollections);
        Assert.Equal(1, summary.FailedPayouts);
        Assert.True(summary.RetentionCleanupCandidates > 0);
    }

    [Fact]
    public async Task RunFailoverDrillAsync_ShouldReturnSuccessfulSteps_WhenDependenciesHealthy()
    {
        await using var context = CreateDbContext(nameof(RunFailoverDrillAsync_ShouldReturnSuccessfulSteps_WhenDependenciesHealthy));
        SeedOperationalData(context);
        await context.SaveChangesAsync();

        var service = new OperationsHardeningService(context, new StubRetentionService());
        var result = await service.RunFailoverDrillAsync();

        Assert.True(result.Successful);
        Assert.Equal(3, result.Steps.Count);
        Assert.All(result.Steps, step => Assert.True(step.Success));
    }

    private static void SeedOperationalData(InsuranceDbContext context)
    {
        var now = DateTime.UtcNow;

        context.Customers.Add(new Customer
        {
            Id = 880,
            CustomerNumber = "CUS-880",
            GhanaCardNumberHash = "hash",
            FirstName = "Kojo",
            LastName = "Mensah",
            DateOfBirth = new DateTime(1992, 1, 1),
            PhoneNumber = "+233540000880",
            Email = "kojo880@example.com",
            ConsentAccepted = true,
            ConsentAcceptedAtUtc = now
        });

        context.Policies.Add(new Policy
        {
            Id = 881,
            PolicyNumber = "POL-OPS-1",
            CustomerId = 880,
            ProductType = ProductType.Motor,
            CoverageType = "Standard",
            PremiumAmount = 100,
            CurrencyCode = "GHS",
            InceptionDate = now.Date.AddMonths(-1),
            ExpiryDate = now.Date.AddMonths(11),
            Status = PolicyStatus.Active
        });

        context.Claims.Add(new Claim
        {
            Id = 882,
            ClaimNumber = "CLM-OPS-1",
            PolicyId = 881,
            IncidentDate = now.Date.AddDays(-4),
            ClaimType = "Accident",
            ClaimedAmount = 150,
            Status = ClaimStatus.UnderReview,
            CreatedAtUtc = now.AddDays(-2),
            UpdatedAtUtc = now.AddDays(-1)
        });

        context.PremiumTransactions.Add(new PremiumTransaction
        {
            PolicyId = 881,
            TransactionReference = "TX-OPS-1",
            Provider = "MTN_MOMO",
            PaymentChannel = "MoMo",
            Amount = 100,
            Status = PaymentStatus.Pending,
            DueDateUtc = now.Date,
            CreatedAtUtc = now.AddDays(-1),
            UpdatedAtUtc = now.AddDays(-1)
        });

        context.PayoutTransactions.Add(new PayoutTransaction
        {
            ClaimId = 882,
            PayoutReference = "PO-OPS-1",
            IdempotencyKey = "IDEM-OPS-1",
            Amount = 50,
            Status = PaymentStatus.Failed,
            DestinationChannel = "MoMo",
            DestinationAccount = "233540001111",
            FailureReason = "Provider timeout",
            CreatedAtUtc = now.AddDays(-1),
            UpdatedAtUtc = now.AddDays(-1)
        });

        context.IdentityAuditLogs.Add(new IdentityAuditLog
        {
            Action = "Login",
            Outcome = "Success",
            SubjectId = "admin",
            Description = "Admin login",
            CorrelationId = "corr-ops",
            IpAddress = "127.0.0.1",
            UserAgent = "Test",
            CreatedAtUtc = now.AddHours(-1),
            UpdatedAtUtc = now.AddHours(-1)
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

    private sealed class StubRetentionService : IDataRetentionService
    {
        public Task<DataRetentionSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new DataRetentionSummaryResponse
            {
                OtpChallengesEligibleForDeletion = 1,
                RefreshTokensEligibleForDeletion = 1,
                IdentityAuditRowsEligibleForAnonymization = 1,
                ClaimNotificationsEligibleForAnonymization = 1,
                PremiumWebhookPayloadsEligibleForRedaction = 1,
                PayoutWebhookPayloadsEligibleForRedaction = 1
            });

        public Task<DataRetentionRunResponse> RunRetentionAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new DataRetentionRunResponse());
    }
}