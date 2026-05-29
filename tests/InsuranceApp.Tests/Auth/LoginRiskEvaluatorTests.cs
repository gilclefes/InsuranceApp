using InsuranceApp.Application.Common;
using InsuranceApp.Contracts.Auth;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Infrastructure.Identity;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InsuranceApp.Tests.Auth;

public class LoginRiskEvaluatorTests
{
    [Fact]
    public async Task RequiresOtpOnLoginAsync_ShouldRequireOtp_ForUnknownDevice()
    {
        await using var context = CreateDbContext(nameof(RequiresOtpOnLoginAsync_ShouldRequireOtp_ForUnknownDevice));
        var evaluator = new LoginRiskEvaluator(context, Options.Create(new AuthOptions
        {
            RequireOtpOnLogin = true,
            OtpForHighRiskDevicesOnly = true,
            TrustedDeviceWindowDays = 90
        }));

        var required = await evaluator.RequiresOtpOnLoginAsync("user-1", new AuthRequestContext
        {
            DeviceId = "new-device"
        });

        Assert.True(required);
    }

    [Fact]
    public async Task RequiresOtpOnLoginAsync_ShouldNotRequireOtp_ForTrustedDevice()
    {
        await using var context = CreateDbContext(nameof(RequiresOtpOnLoginAsync_ShouldNotRequireOtp_ForTrustedDevice));
        context.RefreshTokens.Add(new RefreshToken
        {
            Token = "token-1",
            UserId = "user-1",
            SessionId = "session-1",
            DeviceId = "trusted-device",
            ExpiresAtUtc = DateTime.UtcNow.AddDays(30),
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
            UpdatedAtUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var evaluator = new LoginRiskEvaluator(context, Options.Create(new AuthOptions
        {
            RequireOtpOnLogin = true,
            OtpForHighRiskDevicesOnly = true,
            TrustedDeviceWindowDays = 90
        }));

        var required = await evaluator.RequiresOtpOnLoginAsync("user-1", new AuthRequestContext
        {
            DeviceId = "trusted-device"
        });

        Assert.False(required);
    }

    private static InsuranceDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<InsuranceDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new InsuranceDbContext(options);
    }
}
