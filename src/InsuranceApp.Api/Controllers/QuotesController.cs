using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Quotes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace InsuranceApp.Api.Controllers;

[ApiController]
[EnableRateLimiting("quote")]
[Route("api/v1/quotes")]
public class QuotesController(IQuoteService quoteService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateQuoteRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await quoteService.GenerateQuoteAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpGet("{quoteReference}")]
    public async Task<IActionResult> GetByReference(string quoteReference, CancellationToken cancellationToken)
    {
        try
        {
            var result = await quoteService.GetQuoteAsync(quoteReference, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? productCode,
        [FromQuery] string? status,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        CancellationToken cancellationToken)
    {
        var result = await quoteService.ListQuotesAsync(productCode, status, fromUtc, toUtc, cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("{quoteReference}/reprice")]
    public async Task<IActionResult> Reprice(string quoteReference, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await quoteService.RepriceQuoteAsync(quoteReference, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("{quoteReference}/expire")]
    public async Task<IActionResult> Expire(string quoteReference, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await quoteService.ExpireQuoteAsync(quoteReference, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
