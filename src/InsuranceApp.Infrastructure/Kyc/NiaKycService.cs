using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Kyc;

namespace InsuranceApp.Infrastructure.Kyc;

public class NiaKycService(INiaApiClient niaApiClient) : IKycService
{
    public async Task<GhanaCardVerificationResponse> VerifyGhanaCardAsync(GhanaCardVerificationRequest request, CancellationToken cancellationToken = default)
    {
        return await niaApiClient.VerifyAsync(request, cancellationToken);
    }
}
