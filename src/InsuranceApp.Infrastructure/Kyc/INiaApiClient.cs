using InsuranceApp.Contracts.Kyc;

namespace InsuranceApp.Infrastructure.Kyc;

public interface INiaApiClient
{
    Task<GhanaCardVerificationResponse> VerifyAsync(GhanaCardVerificationRequest request, CancellationToken cancellationToken = default);
}
