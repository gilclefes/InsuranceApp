using InsuranceApp.Contracts.Claims;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Claims;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Tests.Claims;

public class ClaimServiceTests
{
    [Fact]
    public async Task CreateClaimAsync_ShouldCreateFiledClaim()
    {
        await using var context = CreateDbContext(nameof(CreateClaimAsync_ShouldCreateFiledClaim));
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
        context.Policies.Add(new Policy
        {
            PolicyNumber = "POL-CLM-1",
            CustomerId = 1,
            ProductType = ProductType.Motor,
            CoverageType = "Standard",
            PremiumAmount = 200,
            CurrencyCode = "GHS",
            InceptionDate = DateTime.UtcNow.Date.AddMonths(-3),
            ExpiryDate = DateTime.UtcNow.Date.AddMonths(9),
            Status = PolicyStatus.Active
        });
        await context.SaveChangesAsync();

        var service = new ClaimService(context);
        var claim = await service.CreateClaimAsync(new CreateClaimRequest
        {
            PolicyNumber = "POL-CLM-1",
            IncidentDate = DateTime.UtcNow.Date,
            ClaimType = "Accident",
            ClaimedAmount = 1500,
            EvidenceUrl = "https://files.example/evidence-1"
        });

        Assert.StartsWith("CLM-", claim.ClaimNumber, StringComparison.Ordinal);
        Assert.Equal("Filed", claim.Status);
        Assert.Equal("POL-CLM-1", claim.PolicyNumber);
    }

    [Fact]
    public async Task AssignAndReview_ShouldTransitionToApproved()
    {
        await using var context = CreateDbContext(nameof(AssignAndReview_ShouldTransitionToApproved));
        context.Customers.Add(new Customer
        {
            Id = 2,
            CustomerNumber = "CUS-2",
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
            PolicyNumber = "POL-CLM-2",
            CustomerId = 2,
            ProductType = ProductType.Motor,
            CoverageType = "Standard",
            PremiumAmount = 200,
            CurrencyCode = "GHS",
            InceptionDate = DateTime.UtcNow.Date.AddMonths(-3),
            ExpiryDate = DateTime.UtcNow.Date.AddMonths(9),
            Status = PolicyStatus.Active
        });
        await context.SaveChangesAsync();

        var policy = await context.Policies.SingleAsync();
        context.Claims.Add(new Claim
        {
            ClaimNumber = "CLM-TEST-2",
            PolicyId = policy.Id,
            IncidentDate = DateTime.UtcNow.Date,
            ClaimType = "Accident",
            ClaimedAmount = 1000,
            Status = ClaimStatus.Filed,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new ClaimService(context);
        var assigned = await service.AssignClaimAsync("CLM-TEST-2", new AssignClaimRequest
        {
            AdjusterUserId = "adjuster-1",
            Note = "Initial triage"
        });

        Assert.Equal("UnderReview", assigned.Status);
        Assert.Equal("adjuster-1", assigned.AssignedAdjusterId);

        var reviewed = await service.ReviewClaimAsync("CLM-TEST-2", new ReviewClaimRequest
        {
            Action = "approve",
            ApprovedAmount = 800,
            Reason = "Validated damage"
        });

        Assert.Equal("Approved", reviewed.Status);
        Assert.Equal(800, reviewed.ApprovedAmount);
    }

    [Fact]
    public async Task CreateClaimAsync_ShouldFlagFraudRisk_WhenIncidentBeforeInception()
    {
        await using var context = CreateDbContext(nameof(CreateClaimAsync_ShouldFlagFraudRisk_WhenIncidentBeforeInception));
        context.Customers.Add(new Customer
        {
            Id = 10,
            CustomerNumber = "CUS-10",
            GhanaCardNumberHash = "hash",
            FirstName = "Esi",
            LastName = "Boateng",
            DateOfBirth = new DateTime(1992, 1, 1),
            PhoneNumber = "+233200000001",
            Email = "esi@example.com",
            ConsentAccepted = true,
            ConsentAcceptedAtUtc = DateTime.UtcNow
        });
        context.Policies.Add(new Policy
        {
            PolicyNumber = "POL-CLM-FRAUD",
            CustomerId = 10,
            ProductType = ProductType.Motor,
            CoverageType = "Standard",
            PremiumAmount = 100,
            CurrencyCode = "GHS",
            InceptionDate = DateTime.UtcNow.Date,
            ExpiryDate = DateTime.UtcNow.Date.AddYears(1),
            Status = PolicyStatus.Active
        });
        await context.SaveChangesAsync();

        var service = new ClaimService(context);
        var claim = await service.CreateClaimAsync(new CreateClaimRequest
        {
            PolicyNumber = "POL-CLM-FRAUD",
            IncidentDate = DateTime.UtcNow.Date.AddDays(-5),
            ClaimType = "Accident",
            ClaimedAmount = 5000,
            EvidenceUrl = string.Empty
        });

        Assert.True(claim.IsFraudRisk);
        Assert.True(claim.FraudScore >= 60);
        Assert.NotEmpty(claim.FraudReason);
    }

    [Fact]
    public async Task GetTimelineAsync_ShouldReturnEvents_FromCreateAssignReview()
    {
        await using var context = CreateDbContext(nameof(GetTimelineAsync_ShouldReturnEvents_FromCreateAssignReview));
        context.Customers.Add(new Customer
        {
            Id = 11,
            CustomerNumber = "CUS-11",
            GhanaCardNumberHash = "hash",
            FirstName = "Yaw",
            LastName = "Asante",
            DateOfBirth = new DateTime(1991, 1, 1),
            PhoneNumber = "+233200000002",
            Email = "yaw@example.com",
            ConsentAccepted = true,
            ConsentAcceptedAtUtc = DateTime.UtcNow
        });
        context.Policies.Add(new Policy
        {
            PolicyNumber = "POL-CLM-TL",
            CustomerId = 11,
            ProductType = ProductType.Motor,
            CoverageType = "Standard",
            PremiumAmount = 150,
            CurrencyCode = "GHS",
            InceptionDate = DateTime.UtcNow.Date.AddMonths(-1),
            ExpiryDate = DateTime.UtcNow.Date.AddYears(1),
            Status = PolicyStatus.Active
        });
        await context.SaveChangesAsync();

        var service = new ClaimService(context);
        var created = await service.CreateClaimAsync(new CreateClaimRequest
        {
            PolicyNumber = "POL-CLM-TL",
            IncidentDate = DateTime.UtcNow.Date,
            ClaimType = "Theft",
            ClaimedAmount = 300,
            EvidenceUrl = "https://files.example/evidence-2"
        });

        await service.AssignClaimAsync(created.ClaimNumber, new AssignClaimRequest
        {
            AdjusterUserId = "adjuster-2",
            Note = "Assigned for review"
        });

        await service.ReviewClaimAsync(created.ClaimNumber, new ReviewClaimRequest
        {
            Action = "request-documents",
            Reason = "Need police report"
        });

        var timeline = await service.GetTimelineAsync(created.ClaimNumber);
        Assert.True(timeline.Count >= 3);
        Assert.Equal("Created", timeline.First().EventType);
    }

    [Fact]
    public async Task GetSlaDashboardAsync_ShouldReportOpenAndBreachedClaims()
    {
        await using var context = CreateDbContext(nameof(GetSlaDashboardAsync_ShouldReportOpenAndBreachedClaims));
        context.Customers.Add(new Customer
        {
            Id = 3,
            CustomerNumber = "CUS-3",
            GhanaCardNumberHash = "hash",
            FirstName = "Yaw",
            LastName = "Badu",
            DateOfBirth = new DateTime(1988, 4, 4),
            PhoneNumber = "+233242222222",
            Email = "yawb@example.com",
            ConsentAccepted = true,
            ConsentAcceptedAtUtc = DateTime.UtcNow
        });
        context.Policies.Add(new Policy
        {
            PolicyNumber = "POL-SLA-1",
            CustomerId = 3,
            ProductType = ProductType.Motor,
            CoverageType = "Standard",
            PremiumAmount = 200,
            CurrencyCode = "GHS",
            InceptionDate = DateTime.UtcNow.Date.AddMonths(-3),
            ExpiryDate = DateTime.UtcNow.Date.AddMonths(9),
            Status = PolicyStatus.Active
        });
        await context.SaveChangesAsync();

        var policy = await context.Policies.SingleAsync();
        context.Claims.AddRange(
            new Claim
            {
                ClaimNumber = "CLM-SLA-OLD",
                PolicyId = policy.Id,
                IncidentDate = DateTime.UtcNow.Date.AddDays(-4),
                ClaimType = "Accident",
                ClaimedAmount = 100,
                Status = ClaimStatus.Filed,
                CreatedAtUtc = DateTime.UtcNow.AddHours(-100),
                UpdatedAtUtc = DateTime.UtcNow.AddHours(-100)
            },
            new Claim
            {
                ClaimNumber = "CLM-SLA-NEW",
                PolicyId = policy.Id,
                IncidentDate = DateTime.UtcNow.Date,
                ClaimType = "Accident",
                ClaimedAmount = 100,
                Status = ClaimStatus.UnderReview,
                CreatedAtUtc = DateTime.UtcNow.AddHours(-10),
                UpdatedAtUtc = DateTime.UtcNow.AddHours(-10)
            });
        await context.SaveChangesAsync();

        var service = new ClaimService(context);
        var dashboard = await service.GetSlaDashboardAsync(null);

        Assert.Equal(2, dashboard.TotalOpenClaims);
        Assert.Equal(1, dashboard.BreachedClaims);
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