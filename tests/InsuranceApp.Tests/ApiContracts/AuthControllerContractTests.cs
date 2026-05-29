using InsuranceApp.Api.Controllers;
using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsuranceApp.Tests.ApiContracts;

public class AuthControllerContractTests
{
    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenOtpIsRequired()
    {
        var controller = new AuthController(new StubAuthService
        {
            LoginException = new InvalidOperationException("OTP verification is required for this login attempt.")
        }, new NoopAuditLogger());
        controller.ControllerContext = BuildControllerContext();

        var result = await controller.Login(new LoginRequest
        {
            Email = "user@example.com",
            Password = "Password123!"
        }, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
        Assert.Equal("OTP verification is required for this login attempt.", ReadMessage(badRequest.Value));
    }

    [Fact]
    public async Task RevokeAll_ShouldReturnUnauthorized_WhenUserIdMissing()
    {
        var controller = new AuthController(new StubAuthService(), new NoopAuditLogger());
        controller.ControllerContext = BuildControllerContext();

        var result = await controller.RevokeAll(CancellationToken.None);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(401, unauthorized.StatusCode);
        Assert.Equal("User identity could not be resolved from token.", ReadMessage(unauthorized.Value));
    }

    [Fact]
    public async Task RevokeAll_ShouldReturnOk_WhenUserIdPresent()
    {
        var authService = new StubAuthService();
        var controller = new AuthController(authService, new NoopAuditLogger());
        controller.ControllerContext = BuildControllerContext(new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-1")
        }, "test")));

        var result = await controller.RevokeAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Equal("All active sessions revoked.", ReadMessage(ok.Value));
        Assert.Equal("user-1", authService.RevokedUserId);
    }

    private static ControllerContext BuildControllerContext(ClaimsPrincipal? user = null)
    {
        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user ?? new ClaimsPrincipal(new ClaimsIdentity())
            }
        };
    }

    private static string ReadMessage(object? value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        var property = value.GetType().GetProperty("message");
        return property?.GetValue(value)?.ToString() ?? string.Empty;
    }

    private sealed class StubAuthService : IAuthService
    {
        public Exception? LoginException { get; set; }
        public string RevokedUserId { get; private set; } = string.Empty;

        public Task<AuthResponse> RegisterAsync(RegisterRequest request, AuthRequestContext context, CancellationToken cancellationToken = default)
            => Task.FromResult(new AuthResponse());

        public Task<AuthResponse> LoginAsync(LoginRequest request, AuthRequestContext context, CancellationToken cancellationToken = default)
        {
            if (LoginException is not null)
            {
                throw LoginException;
            }

            return Task.FromResult(new AuthResponse());
        }

        public Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, AuthRequestContext context, CancellationToken cancellationToken = default)
            => Task.FromResult(new AuthResponse());

        public Task RevokeAllSessionsAsync(string userId, CancellationToken cancellationToken = default)
        {
            RevokedUserId = userId;
            return Task.CompletedTask;
        }

        public Task<OtpRequestResponse> RequestOtpAsync(OtpRequest request, AuthRequestContext context, CancellationToken cancellationToken = default)
            => Task.FromResult(new OtpRequestResponse());

        public Task<OtpVerifyResponse> VerifyOtpAsync(OtpVerifyRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new OtpVerifyResponse());
    }

    private sealed class NoopAuditLogger : IIdentityAuditLogger
    {
        public Task LogAsync(string action, string outcome, string subjectId, string description, string correlationId, string ipAddress, string userAgent, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
