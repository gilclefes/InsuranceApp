using InsuranceApp.Contracts.Auth;
using InsuranceApp.Contracts.Onboarding;

namespace InsuranceApp.Application.Interfaces;

public interface IOnboardingService
{
    Task<CustomerProfileResponse> CaptureProfileAsync(CaptureProfileRequest request, CancellationToken cancellationToken = default);
    Task<CustomerProfileResponse> AgentOnboardAsync(AgentOnboardCustomerRequest request, AuthRequestContext context, string agentUserId, CancellationToken cancellationToken = default);
}
