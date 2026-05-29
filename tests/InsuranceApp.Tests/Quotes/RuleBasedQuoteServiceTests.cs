using InsuranceApp.Contracts.Quotes;
using InsuranceApp.Infrastructure.Persistence;
using InsuranceApp.Infrastructure.Quotes;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Tests.Quotes;

public class RuleBasedQuoteServiceTests
{
    [Fact]
    public async Task GenerateQuoteAsync_ShouldPersistQuoteRecord()
    {
        await using var context = CreateDbContext(nameof(GenerateQuoteAsync_ShouldPersistQuoteRecord));
        context.Database.EnsureCreated();

        var service = new RuleBasedQuoteService(context);
        var response = await service.GenerateQuoteAsync(new GenerateQuoteRequest
        {
            ProductCode = "MOTOR-TP",
            CoverageAmount = 10000,
            ApplicantAge = 24,
            VehicleValue = 50000,
            SumAssured = 0
        });

        var saved = await context.QuoteRecords.SingleOrDefaultAsync(x => x.QuoteReference == response.QuoteReference);
        Assert.NotNull(saved);
        Assert.Equal(response.TotalPremium, saved!.TotalPremium);
    }

    [Fact]
    public async Task GenerateQuoteAsync_ShouldApplyYoungDriverRule()
    {
        await using var context = CreateDbContext(nameof(GenerateQuoteAsync_ShouldApplyYoungDriverRule));
        context.Database.EnsureCreated();

        var service = new RuleBasedQuoteService(context);
        var response = await service.GenerateQuoteAsync(new GenerateQuoteRequest
        {
            ProductCode = "MOTOR-TP",
            CoverageAmount = 10000,
            ApplicantAge = 22,
            VehicleValue = 8000,
            SumAssured = 0
        });

        Assert.Contains(response.Adjustments, x => x.Reason.Contains("Young driver", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GenerateQuoteAsync_ShouldBeDeterministic_ForIdenticalInputs()
    {
        await using var context = CreateDbContext(nameof(GenerateQuoteAsync_ShouldBeDeterministic_ForIdenticalInputs));
        context.Database.EnsureCreated();

        var service = new RuleBasedQuoteService(context);
        var request = new GenerateQuoteRequest
        {
            ProductCode = "MOTOR-TP",
            CoverageAmount = 10000,
            ApplicantAge = 24,
            VehicleValue = 50000,
            SumAssured = 0
        };

        var first = await service.GenerateQuoteAsync(request);
        var second = await service.GenerateQuoteAsync(request);

        Assert.Equal(first.BasePremium, second.BasePremium);
        Assert.Equal(first.TotalPremium, second.TotalPremium);
    }

    [Fact]
    public async Task RepriceQuoteAsync_ShouldCreateNewQuote_AndMarkPreviousAsRepriced()
    {
        await using var context = CreateDbContext(nameof(RepriceQuoteAsync_ShouldCreateNewQuote_AndMarkPreviousAsRepriced));
        context.Database.EnsureCreated();

        var service = new RuleBasedQuoteService(context);
        var original = await service.GenerateQuoteAsync(new GenerateQuoteRequest
        {
            ProductCode = "MOTOR-TP",
            CoverageAmount = 10000,
            ApplicantAge = 24,
            VehicleValue = 50000,
            SumAssured = 0
        });

        var repriced = await service.RepriceQuoteAsync(original.QuoteReference);

        Assert.NotEqual(original.QuoteReference, repriced.QuoteReference);
        var previous = await context.QuoteRecords.SingleAsync(x => x.QuoteReference == original.QuoteReference);
        Assert.Equal("Repriced", previous.Status);
    }

    [Fact]
    public async Task ExpireQuoteAsync_ShouldMarkQuoteAsExpired()
    {
        await using var context = CreateDbContext(nameof(ExpireQuoteAsync_ShouldMarkQuoteAsExpired));
        context.Database.EnsureCreated();

        var service = new RuleBasedQuoteService(context);
        var generated = await service.GenerateQuoteAsync(new GenerateQuoteRequest
        {
            ProductCode = "MOTOR-TP",
            CoverageAmount = 10000,
            ApplicantAge = 24,
            VehicleValue = 50000,
            SumAssured = 0
        });

        var expired = await service.ExpireQuoteAsync(generated.QuoteReference);

        Assert.Equal("Expired", expired.Status);
    }

    [Fact]
    public async Task GenerateQuoteAsync_ShouldApplySelectedRiderAdjustments()
    {
        await using var context = CreateDbContext(nameof(GenerateQuoteAsync_ShouldApplySelectedRiderAdjustments));
        context.Database.EnsureCreated();

        var service = new RuleBasedQuoteService(context);
        var response = await service.GenerateQuoteAsync(new GenerateQuoteRequest
        {
            ProductCode = "MOTOR-TP",
            CoverageAmount = 10000,
            ApplicantAge = 30,
            VehicleValue = 50000,
            SumAssured = 0,
            SelectedRiderCodes = ["WINDSHIELD"]
        });

        Assert.Contains(response.AppliedRiders, x => x.RiderCode == "WINDSHIELD");
    }

    [Fact]
    public async Task RepriceQuoteAsync_ShouldRetainSelectedRiders()
    {
        await using var context = CreateDbContext(nameof(RepriceQuoteAsync_ShouldRetainSelectedRiders));
        context.Database.EnsureCreated();

        var service = new RuleBasedQuoteService(context);
        var original = await service.GenerateQuoteAsync(new GenerateQuoteRequest
        {
            ProductCode = "MOTOR-TP",
            CoverageAmount = 10000,
            ApplicantAge = 30,
            VehicleValue = 50000,
            SumAssured = 0,
            SelectedRiderCodes = ["WINDSHIELD"]
        });

        var repriced = await service.RepriceQuoteAsync(original.QuoteReference);

        Assert.Contains(repriced.AppliedRiders, x => x.RiderCode == "WINDSHIELD");
    }

    private static InsuranceDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<InsuranceDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new InsuranceDbContext(options);
    }
}
