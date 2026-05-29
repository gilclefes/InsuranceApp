using InsuranceApp.Contracts.Kyc;

namespace InsuranceApp.Application.Interfaces;

public interface IKycService
{
    Task<GhanaCardVerificationResponse> VerifyGhanaCardAsync(GhanaCardVerificationRequest request, CancellationToken cancellationToken = default);
}
