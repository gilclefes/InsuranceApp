using InsuranceApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/admin/data-retention")]
public class DataRetentionController(IDataRetentionService dataRetentionService) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken cancellationToken)
    {
        var summary = await dataRetentionService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run(CancellationToken cancellationToken)
    {
        var result = await dataRetentionService.RunRetentionAsync(cancellationToken);
        return Ok(result);
    }
}