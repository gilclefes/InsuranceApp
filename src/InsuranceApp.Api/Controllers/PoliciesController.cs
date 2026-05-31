using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Policies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/policies")]
public class PoliciesController(IPolicyIssuanceService policyIssuanceService) : ControllerBase
{
    [HttpPost("issue-from-quote")]
    public async Task<IActionResult> IssueFromQuote([FromBody] IssuePolicyFromQuoteRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var response = await policyIssuanceService.IssueFromQuoteAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{policyNumber}/document")]
    public async Task<IActionResult> GetDocument(string policyNumber, CancellationToken cancellationToken)
    {
        try
        {
            var document = await policyIssuanceService.GetPolicyDocumentAsync(policyNumber, cancellationToken);
            return File(document.Content, document.ContentType, document.FileName);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(
        [FromQuery] long? customerId,
        [FromQuery] string? agentUserId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var result = await policyIssuanceService.GetDashboardAsync(customerId, agentUserId, status, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{policyNumber}/endorse")]
    public async Task<IActionResult> Endorse(string policyNumber, [FromBody] EndorsePolicyRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await policyIssuanceService.EndorsePolicyAsync(policyNumber, request, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{policyNumber}/cancel")]
    public async Task<IActionResult> Cancel(string policyNumber, [FromBody] CancelPolicyRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await policyIssuanceService.CancelPolicyAsync(policyNumber, request, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("renewal-reminders/run")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RunRenewalReminders(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await policyIssuanceService.RunRenewalReminderCycleAsync(cancellationToken);
        return Ok(result);
    }
}
