using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Auth;
using InsuranceApp.Contracts.Onboarding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InsuranceApp.Api.Controllers;

[ApiController]
[Route("api/v1/onboarding")]
public class OnboardingController(IOnboardingService onboardingService, IIdentityAuditLogger auditLogger) : ControllerBase
{
    [Authorize]
    [HttpPost("profile")]
    public async Task<IActionResult> CaptureProfile([FromBody] CaptureProfileRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await onboardingService.CaptureProfileAsync(request, cancellationToken);
            await auditLogger.LogAsync("Onboarding.ProfileCapture", "Success", request.Email, "Profile and consent captured.",
                HttpContext.TraceIdentifier,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A",
                Request.Headers.UserAgent.ToString() ?? "N/A",
                cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            await auditLogger.LogAsync("Onboarding.ProfileCapture", "Failed", request.Email, ex.Message,
                HttpContext.TraceIdentifier,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A",
                Request.Headers.UserAgent.ToString() ?? "N/A",
                cancellationToken);
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Agent,Admin")]
    [HttpPost("agent-assisted")]
    public async Task<IActionResult> AgentAssistedOnboarding([FromBody] AgentOnboardCustomerRequest request, CancellationToken cancellationToken)
    {
        var agentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(agentUserId))
        {
            return Unauthorized(new { message = "Agent identity could not be resolved from token." });
        }

        try
        {
            var context = new AuthRequestContext
            {
                DeviceId = Request.Headers.TryGetValue("X-Device-Id", out var deviceHeader) ? deviceHeader.ToString() : string.Empty,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                UserAgent = Request.Headers.UserAgent.ToString()
            };

            var result = await onboardingService.AgentOnboardAsync(request, context, agentUserId, cancellationToken);
            await auditLogger.LogAsync("Onboarding.AgentAssisted", "Success", request.Register.Email, "Agent-assisted onboarding completed.",
                HttpContext.TraceIdentifier,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A",
                Request.Headers.UserAgent.ToString() ?? "N/A",
                cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            await auditLogger.LogAsync("Onboarding.AgentAssisted", "Failed", request.Register.Email, ex.Message,
                HttpContext.TraceIdentifier,
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A",
                Request.Headers.UserAgent.ToString() ?? "N/A",
                cancellationToken);
            return BadRequest(new { message = ex.Message });
        }
    }
}
