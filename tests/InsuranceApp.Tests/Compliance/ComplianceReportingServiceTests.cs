using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Compliance;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Tests.Compliance;

public class ComplianceReportingServiceTests
{
    [Fact]
    public async Task GetSummaryAsync_ShouldAggregateWorkflowMetrics()
    {
        await using var context = CreateDbContext(nameof(GetSummaryAsync_ShouldAggregateWorkflowMetrics));
        SeedDomainData(context);
        await context.SaveChangesAsync();

        var service = new ComplianceReportingService(context);
        var summary = await service.GetSummaryAsync(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow.AddDays(1));

        Assert.Equal(2, summary.PremiumTotalTransactions);
        Assert.Equal(1, summary.PremiumSuccessfulTransactions);
        Assert.Equal(1, summary.PremiumFailedTransactions);

        Assert.Equal(2, summary.PayoutTotalTransactions);
        Assert.Equal(1, summary.PayoutSuccessfulTransactions);
        Assert.Equal(1, summary.PayoutFailedTransactions);

        Assert.Equal(2, summary.ClaimsTotal);
        Assert.Equal(1, summary.ClaimsFraudFlagged);

        Assert.Equal(2, summary.IdentityAuditTotal);
        Assert.Equal(1, summary.IdentityAuditFailures);
        Assert.True(summary.Exceptions.Count >= 3);
    }

    [Fact]
    public async Task ExportSummaryCsvAsync_ShouldIncludeSectionsAndExceptions()
    {
        await using var context = CreateDbContext(nameof(ExportSummaryCsvAsync_ShouldIncludeSectionsAndExceptions));
        SeedDomainData(context);
        await context.SaveChangesAsync();

        var service = new ComplianceReportingService(context);
        var csv = await service.ExportSummaryCsvAsync(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow.AddDays(1));

        Assert.Contains("Section,Metric,Value", csv, StringComparison.Ordinal);
        Assert.Contains("Exceptions", csv, StringComparison.Ordinal);
    }

    private static void SeedDomainData(InsuranceDbContext context)
    {
        var now = DateTime.UtcNow;

        context.Customers.Add(new Customer
        {
            Id = 700,
            CustomerNumber = "CUS-700",
            GhanaCardNumberHash = "hash",
            FirstName = "Abena",
            LastName = "Ofori",
            DateOfBirth = new DateTime(1991, 1, 1),
            PhoneNumber = "+233500000700",
            Email = "abena@example.com",
            ConsentAccepted = true,
            ConsentAcceptedAtUtc = now
        });

        context.Policies.Add(new Policy
        {
            Id = 710,
            PolicyNumber = "POL-COMP-1",
            CustomerId = 700,
            ProductType = ProductType.Motor,
            CoverageType = "Standard",
            PremiumAmount = 250,
            CurrencyCode = "GHS",
            InceptionDate = now.Date.AddMonths(-3),
            ExpiryDate = now.Date.AddMonths(9),
            Status = PolicyStatus.Active
        });

        context.Claims.AddRange(
            new Claim
            {
                Id = 720,
                ClaimNumber = "CLM-COMP-1",
                PolicyId = 710,
                IncidentDate = now.Date.AddDays(-4),
                ClaimType = "Accident",
                ClaimedAmount = 500,
                ApprovedAmount = 300,
                Status = ClaimStatus.Disbursed,
                IsFraudRisk = false,
                CreatedAtUtc = now.AddDays(-3),
                UpdatedAtUtc = now.AddDays(-1)
            },
            new Claim
            {
                Id = 721,
                ClaimNumber = "CLM-COMP-2",
                PolicyId = 710,
                IncidentDate = now.Date.AddDays(-5),
                ClaimType = "Accident",
                ClaimedAmount = 800,
                Status = ClaimStatus.UnderReview,
                IsFraudRisk = true,
                FraudReason = "Incident before inception",
                CreatedAtUtc = now.AddHours(-100),
                UpdatedAtUtc = now.AddHours(-10)
            });

        context.PremiumTransactions.AddRange(
            new PremiumTransaction
            {
                PolicyId = 710,
                TransactionReference = "COL-COMP-1",
                Provider = "MTN_MOMO",
                PaymentChannel = "MoMo",
                Amount = 250,
                Status = PaymentStatus.Success,
                DueDateUtc = now.Date.AddDays(-2),
                ProcessedAtUtc = now.AddDays(-2),
                CreatedAtUtc = now.AddDays(-2),
                UpdatedAtUtc = now.AddDays(-2)
            },
            new PremiumTransaction
            {
                PolicyId = 710,
                TransactionReference = "COL-COMP-2",
                Provider = "MTN_MOMO",
                PaymentChannel = "MoMo",
                Amount = 255,
                Status = PaymentStatus.Failed,
                FailureReason = "Provider timeout",
                DueDateUtc = now.Date.AddDays(-1),
                ProcessedAtUtc = now.AddDays(-1),
                CreatedAtUtc = now.AddDays(-1),
                UpdatedAtUtc = now.AddDays(-1)
            });

        context.PayoutTransactions.AddRange(
            new PayoutTransaction
            {
                ClaimId = 720,
                PayoutReference = "PO-COMP-1",
                IdempotencyKey = "PO-IDEM-1",
                Amount = 300,
                Status = PaymentStatus.Disbursed,
                DestinationChannel = "MoMo",
                DestinationAccount = "233540000111",
                DisbursedAtUtc = now.AddDays(-1),
                CreatedAtUtc = now.AddDays(-2),
                UpdatedAtUtc = now.AddDays(-1)
            },
            new PayoutTransaction
            {
                ClaimId = 721,
                PayoutReference = "PO-COMP-2",
                IdempotencyKey = "PO-IDEM-2",
                Amount = 100,
                Status = PaymentStatus.Failed,
                DestinationChannel = "MoMo",
                DestinationAccount = "233540000222",
                FailureReason = "Provider rejected recipient",
                CreatedAtUtc = now.AddDays(-1),
                UpdatedAtUtc = now.AddDays(-1)
            });

        context.IdentityAuditLogs.AddRange(
            new IdentityAuditLog
            {
                Action = "Login",
                Outcome = "Success",
                SubjectId = "admin",
                Description = "Admin login",
                CorrelationId = "corr-1",
                IpAddress = "127.0.0.1",
                UserAgent = "Test",
                CreatedAtUtc = now.AddHours(-5),
                UpdatedAtUtc = now.AddHours(-5)
            },
            new IdentityAuditLog
            {
                Action = "Login",
                Outcome = "Failure",
                SubjectId = "agent-1",
                Description = "Invalid password",
                CorrelationId = "corr-2",
                IpAddress = "127.0.0.1",
                UserAgent = "Test",
                CreatedAtUtc = now.AddHours(-4),
                UpdatedAtUtc = now.AddHours(-4)
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