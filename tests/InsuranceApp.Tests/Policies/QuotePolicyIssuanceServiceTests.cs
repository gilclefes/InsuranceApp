using InsuranceApp.Contracts.Policies;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Persistence;
using InsuranceApp.Infrastructure.Policies;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Tests.Policies;

public class QuotePolicyIssuanceServiceTests
{
    [Fact]
    public async Task IssueFromQuoteAsync_ShouldGeneratePolicyDocument()
    {
        await using var context = CreateDbContext(nameof(IssueFromQuoteAsync_ShouldGeneratePolicyDocument));
        context.Customers.Add(new Customer
        {
            Id = 1,
            CustomerNumber = "CUS-1",
            GhanaCardNumberHash = "hash",
            FirstName = "Ama",
            LastName = "Mensah",
            DateOfBirth = new DateTime(1990, 1, 1),
            PhoneNumber = "+233240000000",
            Email = "ama@example.com",
            ConsentAccepted = true,
            ConsentAcceptedAtUtc = DateTime.UtcNow
        });
        context.ProductDefinitions.Add(new ProductDefinition
        {
            Id = 999,
            ProductCode = "MOTOR-DOC",
            Name = "Motor Doc",
            ProductType = ProductType.Motor,
            CurrencyCode = "GHS",
            BaseRate = 0.01m,
            MinPremium = 100,
            IsActive = true
        });
        context.QuoteRecords.Add(new QuoteRecord
        {
            QuoteReference = "Q-TEST-DOC",
            ProductCode = "MOTOR-DOC",
            CoverageAmount = 10000,
            ApplicantAge = 30,
            VehicleValue = 50000,
            SumAssured = 0,
            BasePremium = 100,
            TotalPremium = 120,
            CurrencyCode = "GHS",
            ValidUntilUtc = DateTime.UtcNow.AddDays(2),
            Status = "Quoted"
        });
        await context.SaveChangesAsync();

        var service = new QuotePolicyIssuanceService(context);
        var issued = await service.IssueFromQuoteAsync(new IssuePolicyFromQuoteRequest
        {
            QuoteReference = "Q-TEST-DOC",
            CustomerId = 1,
            CoverageType = "Standard",
            InceptionDate = DateTime.UtcNow.Date,
            ExpiryDate = DateTime.UtcNow.Date.AddYears(1)
        });

        Assert.False(string.IsNullOrWhiteSpace(issued.DocumentReference));

        var document = await service.GetPolicyDocumentAsync(issued.PolicyNumber);
        Assert.Equal("application/pdf", document.ContentType);
        Assert.NotEmpty(document.Content);

        var notifications = await context.PolicyNotifications.Where(x => x.TemplateKey == "PolicyIssued").ToListAsync();
        Assert.True(notifications.Count >= 3);
    }

    [Fact]
    public async Task RunRenewalReminderCycleAsync_ShouldCreateReminderNotifications()
    {
        await using var context = CreateDbContext(nameof(RunRenewalReminderCycleAsync_ShouldCreateReminderNotifications));
        context.Customers.Add(new Customer
        {
            Id = 10,
            CustomerNumber = "CUS-10",
            GhanaCardNumberHash = "hash",
            FirstName = "Kojo",
            LastName = "Owusu",
            DateOfBirth = new DateTime(1991, 2, 2),
            PhoneNumber = "+233241111111",
            Email = "kojo@example.com",
            ConsentAccepted = true,
            ConsentAcceptedAtUtc = DateTime.UtcNow
        });
        context.Policies.Add(new Policy
        {
            PolicyNumber = "POL-REN-1",
            CustomerId = 10,
            ProductType = ProductType.Motor,
            CoverageType = "Standard",
            PremiumAmount = 120,
            CurrencyCode = "GHS",
            InceptionDate = DateTime.UtcNow.Date.AddMonths(-11),
            ExpiryDate = DateTime.UtcNow.Date.AddDays(10),
            Status = PolicyStatus.Active
        });
        await context.SaveChangesAsync();

        var service = new QuotePolicyIssuanceService(context);
        var run = await service.RunRenewalReminderCycleAsync();

        Assert.Equal(1, run.PoliciesScanned);
        Assert.Equal(1, run.RemindersSent);
        var reminders = await context.PolicyNotifications.Where(x => x.TemplateKey == "RenewalReminder").ToListAsync();
        Assert.True(reminders.Count >= 3);
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
