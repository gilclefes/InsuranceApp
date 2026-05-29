using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.PremiumCollections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/premium-collections")]
public class PremiumCollectionsController(IPremiumCollectionService premiumCollectionService) : ControllerBase
{
    [HttpPost("mandates")]
    public async Task<IActionResult> CreateMandate([FromBody] CreatePremiumMandateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await premiumCollectionService.CreateMandateAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{policyNumber}/schedule")]
    public async Task<IActionResult> Schedule(string policyNumber, [FromBody] SchedulePremiumCollectionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await premiumCollectionService.ScheduleCollectionAsync(policyNumber, request, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{policyNumber}")]
    public async Task<IActionResult> ListByPolicy(string policyNumber, CancellationToken cancellationToken)
    {
        var result = await premiumCollectionService.ListPolicyCollectionsAsync(policyNumber, cancellationToken);
        return Ok(result);
    }

    [HttpPost("run-due")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RunDue(CancellationToken cancellationToken)
    {
        var result = await premiumCollectionService.RunDueCollectionsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("retry-failed")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RetryFailed(CancellationToken cancellationToken)
    {
        var result = await premiumCollectionService.RetryFailedCollectionsAsync(cancellationToken);
        return Ok(result);
    }
}