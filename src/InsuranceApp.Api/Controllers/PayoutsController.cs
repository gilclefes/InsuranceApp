using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Payouts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/payouts")]
public class PayoutsController(IPayoutService payoutService) : ControllerBase
{
    [HttpPost("initiate")]
    [Authorize(Roles = "Admin,Agent")]
    public async Task<IActionResult> Initiate([FromBody] InitiatePayoutRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await payoutService.InitiatePayoutAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{payoutReference}")]
    public async Task<IActionResult> GetByReference(string payoutReference, CancellationToken cancellationToken)
    {
        try
        {
            var result = await payoutService.GetPayoutAsync(payoutReference, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? status, [FromQuery] string? claimNumber, [FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc, CancellationToken cancellationToken)
    {
        var result = await payoutService.ListPayoutsAsync(status, claimNumber, fromUtc, toUtc, cancellationToken);
        return Ok(result);
    }

    [HttpPost("reconcile/run")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RunReconciliation(CancellationToken cancellationToken)
    {
        var result = await payoutService.RunReconciliationAsync(cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("webhooks/provider")]
    public async Task<IActionResult> ProcessWebhook([FromBody] PayoutWebhookRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await payoutService.ProcessWebhookAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}