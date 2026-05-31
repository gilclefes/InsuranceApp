using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/claims")]
public class ClaimsController(IClaimService claimService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClaimRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await claimService.CreateClaimAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{claimNumber}")]
    public async Task<IActionResult> GetByNumber(string claimNumber, CancellationToken cancellationToken)
    {
        try
        {
            var result = await claimService.GetClaimAsync(claimNumber, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? status, [FromQuery] string? assignedAdjusterId, [FromQuery] string? policyNumber, CancellationToken cancellationToken)
    {
        var result = await claimService.ListClaimsAsync(status, assignedAdjusterId, policyNumber, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{claimNumber}/assign")]
    [Authorize(Roles = "Admin,Agent")]
    public async Task<IActionResult> Assign(string claimNumber, [FromBody] AssignClaimRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await claimService.AssignClaimAsync(claimNumber, request, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{claimNumber}/review")]
    [Authorize(Roles = "Admin,Agent")]
    public async Task<IActionResult> Review(string claimNumber, [FromBody] ReviewClaimRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await claimService.ReviewClaimAsync(claimNumber, request, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{claimNumber}/timeline")]
    public async Task<IActionResult> Timeline(string claimNumber, CancellationToken cancellationToken)
    {
        try
        {
            var result = await claimService.GetTimelineAsync(claimNumber, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("sla-dashboard")]
    [Authorize(Roles = "Admin,Agent")]
    public async Task<IActionResult> SlaDashboard([FromQuery] string? assignedAdjusterId, CancellationToken cancellationToken)
    {
        var result = await claimService.GetSlaDashboardAsync(assignedAdjusterId, cancellationToken);
        return Ok(result);
    }
}