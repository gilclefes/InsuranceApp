using InsuranceApp.Application.Interfaces;
using InsuranceApp.Application.Common;
using InsuranceApp.Contracts.Auth;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Infrastructure.Persistence;
using InsuranceApp.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace InsuranceApp.Infrastructure.Identity;

public class IdentityAuthService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    InsuranceDbContext dbContext,
    JwtTokenFactory tokenFactory,
    IOptions<JwtOptions> jwtOptions,
    LoginRiskEvaluator loginRiskEvaluator,
    IOtpService otpService) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, AuthRequestContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException("Email and password are required.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existing = await userManager.FindByEmailAsync(normalizedEmail);
        if (existing is not null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var role = string.IsNullOrWhiteSpace(request.Role) ? "Customer" : request.Role.Trim();
        if (!await roleManager.RoleExistsAsync(role))
        {
            var roleCreateResult = await roleManager.CreateAsync(new IdentityRole(role));
            if (!roleCreateResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", roleCreateResult.Errors.Select(x => x.Description)));
            }
        }

        var user = new ApplicationUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            PhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", createResult.Errors.Select(x => x.Description)));
        }

        var addRoleResult = await userManager.AddToRoleAsync(user, role);
        if (!addRoleResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", addRoleResult.Errors.Select(x => x.Description)));
        }

        return await CreateAuthResponseAsync(user, role, context, sessionId: null, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, AuthRequestContext context, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await userManager.FindByEmailAsync(normalizedEmail)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        var isValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!isValid)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (await loginRiskEvaluator.RequiresOtpOnLoginAsync(user.Id, context, cancellationToken))
        {
            if (string.IsNullOrWhiteSpace(request.OtpChallengeId) || string.IsNullOrWhiteSpace(request.OtpCode))
            {
                throw new InvalidOperationException("OTP verification is required for this login attempt.");
            }

            var otpResult = await otpService.VerifyAsync(new OtpVerifyRequest
            {
                ChallengeId = request.OtpChallengeId,
                Code = request.OtpCode
            }, cancellationToken);

            if (!otpResult.IsVerified)
            {
                throw new UnauthorizedAccessException(otpResult.Message);
            }
        }

        var roles = await userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? "Customer";

        return await CreateAuthResponseAsync(user, primaryRole, context, sessionId: null, cancellationToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, AuthRequestContext context, CancellationToken cancellationToken = default)
    {
        var refreshToken = await dbContext.RefreshTokens
            .Where(x => x.Token == request.RefreshToken)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (!refreshToken.IsActive)
        {
            throw new UnauthorizedAccessException("Refresh token is expired or revoked.");
        }

        if (!string.IsNullOrWhiteSpace(request.SessionId) && !string.Equals(refreshToken.SessionId, request.SessionId, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Refresh token session mismatch.");
        }

        if (!string.IsNullOrWhiteSpace(refreshToken.DeviceId) && !string.IsNullOrWhiteSpace(context.DeviceId)
            && !string.Equals(refreshToken.DeviceId, context.DeviceId, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Refresh token device mismatch.");
        }

        var user = await userManager.FindByIdAsync(refreshToken.UserId)
            ?? throw new UnauthorizedAccessException("User not found for refresh token.");

        refreshToken.RevokedAtUtc = DateTime.UtcNow;
        refreshToken.RevokedReason = "Rotated";

        var roles = await userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? "Customer";

        var response = await CreateAuthResponseAsync(user, primaryRole, context, refreshToken.SessionId, cancellationToken);
        refreshToken.ReplacedByToken = response.RefreshToken;
        await dbContext.SaveChangesAsync(cancellationToken);

        return response;
    }

    public async Task RevokeAllSessionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var activeTokens = await dbContext.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAtUtc == null && x.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAtUtc = DateTime.UtcNow;
            token.RevokedReason = "UserRequestedRevokeAll";
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<OtpRequestResponse> RequestOtpAsync(OtpRequest request, AuthRequestContext context, CancellationToken cancellationToken = default)
    {
        return otpService.RequestAsync(request, context, cancellationToken);
    }

    public Task<OtpVerifyResponse> VerifyOtpAsync(OtpVerifyRequest request, CancellationToken cancellationToken = default)
    {
        return otpService.VerifyAsync(request, cancellationToken);
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(
        ApplicationUser user,
        string role,
        AuthRequestContext context,
        string? sessionId,
        CancellationToken cancellationToken)
    {
        var (accessToken, expiresAtUtc) = tokenFactory.CreateToken(user.Id, user.Email ?? string.Empty, role);
        var refreshTokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var resolvedSessionId = string.IsNullOrWhiteSpace(sessionId) ? Guid.NewGuid().ToString("N") : sessionId;

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshTokenValue,
            UserId = user.Id,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays),
            SessionId = resolvedSessionId,
            DeviceId = context.DeviceId,
            IpAddress = context.IpAddress,
            UserAgent = context.UserAgent
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            SessionId = resolvedSessionId,
            ExpiresAtUtc = expiresAtUtc,
            Role = role
        };
    }
}
