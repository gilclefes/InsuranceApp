using InsuranceApp.Application.Common;
using InsuranceApp.Contracts.Auth;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InsuranceApp.Infrastructure.Identity;

public class LoginRiskEvaluator(InsuranceDbContext dbContext, IOptions<AuthOptions> authOptions)
{
    private readonly AuthOptions _authOptions = authOptions.Value;

    public async Task<bool> RequiresOtpOnLoginAsync(string userId, AuthRequestContext context, CancellationToken cancellationToken = default)
    {
        if (!_authOptions.RequireOtpOnLogin)
        {
            return false;
        }

        if (!_authOptions.OtpForHighRiskDevicesOnly)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(context.DeviceId))
        {
            return true;
        }

        var trustedSince = DateTime.UtcNow.AddDays(-Math.Abs(_authOptions.TrustedDeviceWindowDays));
        var knownDeviceExists = await dbContext.RefreshTokens.AnyAsync(
            x => x.UserId == userId
                 && x.DeviceId == context.DeviceId
                 && x.CreatedAtUtc >= trustedSince,
            cancellationToken);

        return !knownDeviceExists;
    }
}
