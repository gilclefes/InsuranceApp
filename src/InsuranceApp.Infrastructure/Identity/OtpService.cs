using InsuranceApp.Application.Common;
using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Auth;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Infrastructure.Otp;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace InsuranceApp.Infrastructure.Identity;

public class OtpService(
    InsuranceDbContext dbContext,
    IOtpSender otpSender,
    IOptions<OtpOptions> options) : IOtpService
{
    private readonly OtpOptions _options = options.Value;

    public async Task<OtpRequestResponse> RequestAsync(OtpRequest request, AuthRequestContext context, CancellationToken cancellationToken = default)
    {
        var destination = request.Destination.Trim();
        if (string.IsNullOrWhiteSpace(destination))
        {
            throw new InvalidOperationException("OTP destination is required.");
        }

        var now = DateTime.UtcNow;
        var recentCount = await dbContext.OtpChallenges
            .CountAsync(x => x.Destination == destination
                && x.Purpose == request.Purpose
                && x.CreatedAtUtc >= now.AddHours(-1), cancellationToken);

        if (recentCount >= _options.MaxRequestsPerHour)
        {
            throw new InvalidOperationException("Too many OTP requests. Try again later.");
        }

        var latest = await dbContext.OtpChallenges
            .Where(x => x.Destination == destination && x.Purpose == request.Purpose)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (latest is not null)
        {
            var secondsSinceLastRequest = (int)(now - latest.LastSentAtUtc).TotalSeconds;
            if (secondsSinceLastRequest < _options.ThrottleSeconds)
            {
                throw new InvalidOperationException($"Please wait {_options.ThrottleSeconds - secondsSinceLastRequest} seconds before requesting another OTP.");
            }
        }

        var otpCode = GenerateNumericCode(_options.CodeLength);
        var challenge = new OtpChallenge
        {
            ChallengeId = Guid.NewGuid().ToString("N"),
            Destination = destination,
            Purpose = request.Purpose,
            Channel = request.Channel,
            CodeHash = HashCode(otpCode),
            ExpiresAtUtc = now.AddMinutes(_options.ExpiryMinutes),
            MaxAttempts = _options.MaxAttempts,
            AttemptCount = 0,
            LastSentAtUtc = now,
            IpAddress = context.IpAddress,
            UserAgent = context.UserAgent
        };

        dbContext.OtpChallenges.Add(challenge);
        await dbContext.SaveChangesAsync(cancellationToken);

        await otpSender.SendAsync(destination, request.Channel, $"Your OTP is {otpCode}", cancellationToken);

        return new OtpRequestResponse
        {
            ChallengeId = challenge.ChallengeId,
            ExpiresAtUtc = challenge.ExpiresAtUtc,
            RetryAfterSeconds = _options.ThrottleSeconds
        };
    }

    public async Task<OtpVerifyResponse> VerifyAsync(OtpVerifyRequest request, CancellationToken cancellationToken = default)
    {
        var challenge = await dbContext.OtpChallenges
            .Where(x => x.ChallengeId == request.ChallengeId)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("OTP challenge not found.");

        if (challenge.IsLocked)
        {
            return new OtpVerifyResponse
            {
                IsVerified = false,
                IsLocked = true,
                RemainingAttempts = 0,
                Message = "OTP challenge is locked. Please request a new code later."
            };
        }

        if (challenge.IsExpired)
        {
            return new OtpVerifyResponse
            {
                IsVerified = false,
                IsLocked = false,
                RemainingAttempts = 0,
                Message = "OTP code has expired."
            };
        }

        if (challenge.IsVerified)
        {
            return new OtpVerifyResponse
            {
                IsVerified = true,
                IsLocked = false,
                RemainingAttempts = challenge.MaxAttempts - challenge.AttemptCount,
                Message = "OTP already verified."
            };
        }

        var incomingHash = HashCode(request.Code);
        if (incomingHash == challenge.CodeHash)
        {
            challenge.VerifiedAtUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);

            return new OtpVerifyResponse
            {
                IsVerified = true,
                IsLocked = false,
                RemainingAttempts = challenge.MaxAttempts - challenge.AttemptCount,
                Message = "OTP verified successfully."
            };
        }

        challenge.AttemptCount++;
        if (challenge.AttemptCount >= challenge.MaxAttempts)
        {
            challenge.LockedUntilUtc = DateTime.UtcNow.AddMinutes(_options.LockoutMinutes);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new OtpVerifyResponse
        {
            IsVerified = false,
            IsLocked = challenge.IsLocked,
            RemainingAttempts = Math.Max(0, challenge.MaxAttempts - challenge.AttemptCount),
            Message = challenge.IsLocked
                ? "Too many invalid attempts. OTP challenge locked."
                : "Invalid OTP code."
        };
    }

    private static string GenerateNumericCode(int length)
    {
        var bytes = RandomNumberGenerator.GetBytes(length);
        var chars = bytes.Select(b => (char)('0' + (b % 10))).ToArray();
        return new string(chars);
    }

    private static string HashCode(string code)
    {
        var bytes = Encoding.UTF8.GetBytes(code.Trim());
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
