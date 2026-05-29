using InsuranceApp.Api.Controllers;
using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Auth;
using InsuranceApp.Contracts.Onboarding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsuranceApp.Tests.ApiContracts;

public class OnboardingControllerContractTests
{
    [Fact]
    public async Task AgentAssistedOnboarding_ShouldReturnUnauthorized_WhenAgentIdMissing()
    {
        var controller = new OnboardingController(new StubOnboardingService(), new NoopAuditLogger())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            }
        };

        var result = await controller.AgentAssistedOnboarding(new AgentOnboardCustomerRequest(), CancellationToken.None);
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Agent identity could not be resolved from token.", ReadMessage(unauthorized.Value));
    }

    [Fact]
    public async Task CaptureProfile_ShouldReturnBadRequest_WhenConsentMissing()
    {
        var controller = new OnboardingController(new StubOnboardingService
        {
            CaptureException = new InvalidOperationException("Consent must be accepted before profile can be saved.")
        }, new NoopAuditLogger())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") }))
                }
            }
        };

        var result = await controller.CaptureProfile(new CaptureProfileRequest
        {
            Email = "ama@example.com"
        }, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Consent must be accepted before profile can be saved.", ReadMessage(badRequest.Value));
    }

    private static string ReadMessage(object? value)
    {
        var property = value?.GetType().GetProperty("message");
        return property?.GetValue(value)?.ToString() ?? string.Empty;
    }

    private sealed class StubOnboardingService : IOnboardingService
    {
        public Exception? CaptureException { get; set; }

        public Task<CustomerProfileResponse> CaptureProfileAsync(CaptureProfileRequest request, CancellationToken cancellationToken = default)
        {
            if (CaptureException is not null)
            {
                throw CaptureException;
            }

            return Task.FromResult(new CustomerProfileResponse());
        }

        public Task<CustomerProfileResponse> AgentOnboardAsync(AgentOnboardCustomerRequest request, AuthRequestContext context, string agentUserId, CancellationToken cancellationToken = default)
            => Task.FromResult(new CustomerProfileResponse());
    }

    private sealed class NoopAuditLogger : IIdentityAuditLogger
    {
        public Task LogAsync(string action, string outcome, string subjectId, string description, string correlationId, string ipAddress, string userAgent, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
