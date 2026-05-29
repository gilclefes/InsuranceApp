using InsuranceApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/admin/operations-hardening")]
public class OperationsHardeningController(IOperationsHardeningService operationsHardeningService) : ControllerBase
{
    [HttpGet("readiness")]
    public async Task<IActionResult> Readiness(CancellationToken cancellationToken)
    {
        var result = await operationsHardeningService.GetReadinessSummaryAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("failover-drill/run")]
    public async Task<IActionResult> RunFailoverDrill(CancellationToken cancellationToken)
    {
        var result = await operationsHardeningService.RunFailoverDrillAsync(cancellationToken);
        return Ok(result);
    }
}