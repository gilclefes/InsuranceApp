using InsuranceApp.Contracts.Auth;

namespace InsuranceApp.Application.Interfaces;

public interface IOtpService
{
    Task<OtpRequestResponse> RequestAsync(OtpRequest request, AuthRequestContext context, CancellationToken cancellationToken = default);
    Task<OtpVerifyResponse> VerifyAsync(OtpVerifyRequest request, CancellationToken cancellationToken = default);
}
