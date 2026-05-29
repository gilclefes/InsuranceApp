using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace InsuranceApp.Api.Controllers;

[ApiController]
[EnableRateLimiting("auth")]
[Route("api/v1/auth")]
public class AuthController(IAuthService authService, IIdentityAuditLogger auditLogger) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.RegisterAsync(request, BuildContext(), cancellationToken);
            await AuditAsync("Auth.Register", "Success", request.Email, "User registration completed.", cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            await AuditAsync("Auth.Register", "Failed", request.Email, ex.Message, cancellationToken);
            return BadRequest(new { message = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.LoginAsync(request, BuildContext(), cancellationToken);
            await AuditAsync("Auth.Login", "Success", request.Email, "User login completed.", cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            await AuditAsync("Auth.Login", "Failed", request.Email, ex.Message, cancellationToken);
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            await AuditAsync("Auth.Login", "Failed", request.Email, ex.Message, cancellationToken);
            return Unauthorized(new { message = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.RefreshTokenAsync(request, BuildContext(), cancellationToken);
            await AuditAsync("Auth.Refresh", "Success", request.SessionId, "Token refresh completed.", cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            await AuditAsync("Auth.Refresh", "Failed", request.SessionId, ex.Message, cancellationToken);
            return Unauthorized(new { message = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("request-otp")]
    public async Task<IActionResult> RequestOtp([FromBody] OtpRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.RequestOtpAsync(request, BuildContext(), cancellationToken);
            await AuditAsync("Auth.OtpRequest", "Success", request.Destination, "OTP challenge issued.", cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            await AuditAsync("Auth.OtpRequest", "Failed", request.Destination, ex.Message, cancellationToken);
            return BadRequest(new { message = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] OtpVerifyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.VerifyOtpAsync(request, cancellationToken);
            var outcome = response.IsVerified ? "Success" : "Failed";
            await AuditAsync("Auth.OtpVerify", outcome, request.ChallengeId, response.Message, cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            await AuditAsync("Auth.OtpVerify", "Failed", request.ChallengeId, ex.Message, cancellationToken);
            return Unauthorized(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("revoke-all")]
    public async Task<IActionResult> RevokeAll(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
        {
            await AuditAsync("Auth.RevokeAll", "Failed", "anonymous", "User identity could not be resolved from token.", cancellationToken);
            return Unauthorized(new { message = "User identity could not be resolved from token." });
        }

        await authService.RevokeAllSessionsAsync(userId, cancellationToken);
        await AuditAsync("Auth.RevokeAll", "Success", userId, "All active sessions revoked.", cancellationToken);
        return Ok(new { message = "All active sessions revoked." });
    }

    private Task AuditAsync(string action, string outcome, string subjectId, string description, CancellationToken cancellationToken)
    {
        var correlationId = Response.Headers.TryGetValue("X-Correlation-Id", out var header)
            ? header.ToString()
            : HttpContext.TraceIdentifier;

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var userAgent = Request.Headers.UserAgent.ToString();

        return auditLogger.LogAsync(
            action,
            outcome,
            string.IsNullOrWhiteSpace(subjectId) ? "unknown" : subjectId,
            string.IsNullOrWhiteSpace(description) ? "N/A" : description,
            string.IsNullOrWhiteSpace(correlationId) ? "N/A" : correlationId,
            string.IsNullOrWhiteSpace(ipAddress) ? "N/A" : ipAddress,
            string.IsNullOrWhiteSpace(userAgent) ? "N/A" : userAgent,
            cancellationToken);
    }

    private AuthRequestContext BuildContext()
    {
        return new AuthRequestContext
        {
            DeviceId = Request.Headers.TryGetValue("X-Device-Id", out var deviceHeader)
                ? deviceHeader.ToString()
                : string.Empty,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            UserAgent = Request.Headers.UserAgent.ToString()
        };
    }
}
