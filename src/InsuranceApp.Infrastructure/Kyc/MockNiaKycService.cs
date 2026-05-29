using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Kyc;
using System.Text.RegularExpressions;

namespace InsuranceApp.Infrastructure.Kyc;

public class MockNiaKycService : IKycService
{
    private static readonly Regex GhanaCardRegex = new("^GHA-\\d{9}-\\d$", RegexOptions.Compiled);

    public Task<GhanaCardVerificationResponse> VerifyGhanaCardAsync(GhanaCardVerificationRequest request, CancellationToken cancellationToken = default)
    {
        var isFormatValid = GhanaCardRegex.IsMatch(request.GhanaCardNumber.Trim());
        if (!isFormatValid)
        {
            return Task.FromResult(new GhanaCardVerificationResponse
            {
                IsVerified = false,
                VerificationStatus = "Failed",
                Message = "Invalid Ghana Card format. Expected format: GHA-123456789-1."
            });
        }

        return Task.FromResult(new GhanaCardVerificationResponse
        {
            IsVerified = true,
            VerificationStatus = "Verified",
            Message = "Verification successful in mock mode. Replace with NIA API adapter in integration environment."
        });
    }
}
