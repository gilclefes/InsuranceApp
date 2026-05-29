using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Kyc;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Api.Controllers;

[ApiController]
[Route("api/v1/kyc")]
public class KycController(IKycService kycService, IIdentityAuditLogger auditLogger) : ControllerBase
{
    [HttpPost("verify-ghana-card")]
    public async Task<IActionResult> VerifyGhanaCard([FromBody] GhanaCardVerificationRequest request, CancellationToken cancellationToken)
    {
        var result = await kycService.VerifyGhanaCardAsync(request, cancellationToken);
        await auditLogger.LogAsync(
            "Kyc.VerifyGhanaCard",
            result.IsVerified ? "Success" : "Pending",
            request.GhanaCardNumber,
            result.Message,
            HttpContext.TraceIdentifier,
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A",
            Request.Headers.UserAgent.ToString() ?? "N/A",
            cancellationToken);

        return Ok(result);
    }
}
