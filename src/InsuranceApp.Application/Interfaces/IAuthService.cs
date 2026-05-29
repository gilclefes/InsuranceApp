using InsuranceApp.Contracts.Auth;

namespace InsuranceApp.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, AuthRequestContext context, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, AuthRequestContext context, CancellationToken cancellationToken = default);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, AuthRequestContext context, CancellationToken cancellationToken = default);
    Task RevokeAllSessionsAsync(string userId, CancellationToken cancellationToken = default);
    Task<OtpRequestResponse> RequestOtpAsync(OtpRequest request, AuthRequestContext context, CancellationToken cancellationToken = default);
    Task<OtpVerifyResponse> VerifyOtpAsync(OtpVerifyRequest request, CancellationToken cancellationToken = default);
}
