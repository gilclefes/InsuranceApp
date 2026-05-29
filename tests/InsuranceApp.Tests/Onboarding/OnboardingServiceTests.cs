using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Auth;
using InsuranceApp.Contracts.Onboarding;
using InsuranceApp.Infrastructure.Onboarding;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Tests.Onboarding;

public class OnboardingServiceTests
{
    [Fact]
    public async Task CaptureProfileAsync_ShouldFail_WhenConsentNotAccepted()
    {
        await using var context = CreateDbContext(nameof(CaptureProfileAsync_ShouldFail_WhenConsentNotAccepted));
        var service = new OnboardingService(context, new StubAuthService());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CaptureProfileAsync(new CaptureProfileRequest
        {
            Email = "ama@example.com",
            FirstName = "Ama",
            LastName = "Mensah",
            DateOfBirth = new DateTime(1990, 1, 1),
            PhoneNumber = "+233240000000",
            GhanaCardNumber = "GHA-123456789-1",
            ConsentAccepted = false
        }));
    }

    [Fact]
    public async Task AgentOnboardAsync_ShouldCreateCustomer_AndMarkAgentMetadata()
    {
        await using var context = CreateDbContext(nameof(AgentOnboardAsync_ShouldCreateCustomer_AndMarkAgentMetadata));
        var service = new OnboardingService(context, new StubAuthService());

        var response = await service.AgentOnboardAsync(new AgentOnboardCustomerRequest
        {
            Register = new RegisterRequest
            {
                FirstName = "Kojo",
                LastName = "Owusu",
                Email = "kojo@example.com",
                PhoneNumber = "+233241111111",
                Password = "Password123!"
            },
            DateOfBirth = new DateTime(1992, 5, 20),
            GhanaCardNumber = "GHA-123456789-1",
            ConsentAccepted = true
        }, new AuthRequestContext(), "agent-1");

        Assert.True(response.ConsentAccepted);
        Assert.True(response.RegisteredByAgent);
        Assert.Equal("agent-1", response.RegisteredByAgentId);

        var customer = await context.Customers.SingleAsync(x => x.Email == "kojo@example.com");
        Assert.True(customer.RegisteredByAgent);
    }

    private static InsuranceDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<InsuranceDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new InsuranceDbContext(options);
    }

    private sealed class StubAuthService : IAuthService
    {
        public Task<AuthResponse> RegisterAsync(RegisterRequest request, AuthRequestContext context, CancellationToken cancellationToken = default)
            => Task.FromResult(new AuthResponse());

        public Task<AuthResponse> LoginAsync(LoginRequest request, AuthRequestContext context, CancellationToken cancellationToken = default)
            => Task.FromResult(new AuthResponse());

        public Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, AuthRequestContext context, CancellationToken cancellationToken = default)
            => Task.FromResult(new AuthResponse());

        public Task RevokeAllSessionsAsync(string userId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<OtpRequestResponse> RequestOtpAsync(OtpRequest request, AuthRequestContext context, CancellationToken cancellationToken = default)
            => Task.FromResult(new OtpRequestResponse());

        public Task<OtpVerifyResponse> VerifyOtpAsync(OtpVerifyRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new OtpVerifyResponse());
    }
}
