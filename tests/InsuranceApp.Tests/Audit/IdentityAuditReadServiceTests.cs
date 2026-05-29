using InsuranceApp.Contracts.Audit;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Infrastructure.Identity;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Tests.Audit;

public class IdentityAuditReadServiceTests
{
    [Fact]
    public async Task QueryAsync_ShouldFilterByActionAndOutcome()
    {
        await using var context = CreateDbContext(nameof(QueryAsync_ShouldFilterByActionAndOutcome));
        context.IdentityAuditLogs.AddRange(
            new IdentityAuditLog
            {
                Action = "Auth.Login",
                Outcome = "Success",
                SubjectId = "user-1",
                Description = "ok",
                CorrelationId = "c1",
                IpAddress = "127.0.0.1",
                UserAgent = "test",
                CreatedAtUtc = DateTime.UtcNow.AddMinutes(-5)
            },
            new IdentityAuditLog
            {
                Action = "Auth.Login",
                Outcome = "Failed",
                SubjectId = "user-2",
                Description = "bad",
                CorrelationId = "c2",
                IpAddress = "127.0.0.1",
                UserAgent = "test",
                CreatedAtUtc = DateTime.UtcNow.AddMinutes(-1)
            },
            new IdentityAuditLog
            {
                Action = "Auth.Register",
                Outcome = "Success",
                SubjectId = "user-3",
                Description = "ok",
                CorrelationId = "c3",
                IpAddress = "127.0.0.1",
                UserAgent = "test",
                CreatedAtUtc = DateTime.UtcNow
            });
        await context.SaveChangesAsync();

        var service = new IdentityAuditReadService(context);
        var result = await service.QueryAsync(new IdentityAuditLogQueryRequest
        {
            Action = "Auth.Login",
            Outcome = "Failed",
            Page = 1,
            PageSize = 10
        });

        Assert.Equal(1, result.TotalCount);
        var item = Assert.Single(result.Items);
        Assert.Equal("Auth.Login", item.Action);
        Assert.Equal("Failed", item.Outcome);
    }

    [Fact]
    public async Task ExportCsvAsync_ShouldReturnHeaderAndRows()
    {
        await using var context = CreateDbContext(nameof(ExportCsvAsync_ShouldReturnHeaderAndRows));
        context.IdentityAuditLogs.Add(new IdentityAuditLog
        {
            Action = "Kyc.VerifyGhanaCard",
            Outcome = "Pending",
            SubjectId = "GHA-123",
            Description = "provider timeout",
            CorrelationId = "cid-1",
            IpAddress = "10.0.0.1",
            UserAgent = "api-test",
            CreatedAtUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new IdentityAuditReadService(context);
        var csv = await service.ExportCsvAsync(new IdentityAuditLogQueryRequest());

        Assert.Contains("Id,CreatedAtUtc,Action,Outcome,SubjectId,Description,CorrelationId,IpAddress,UserAgent", csv, StringComparison.Ordinal);
        Assert.Contains("Kyc.VerifyGhanaCard", csv, StringComparison.Ordinal);
    }

    private static InsuranceDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<InsuranceDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new InsuranceDbContext(options);
    }
}
