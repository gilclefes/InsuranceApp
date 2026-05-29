using System.Text;
using InsuranceApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/admin/compliance-reports")]
public class ComplianceReportsController(IComplianceReportingService complianceReportingService) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc, CancellationToken cancellationToken)
    {
        var summary = await complianceReportingService.GetSummaryAsync(fromUtc, toUtc, cancellationToken);
        return Ok(summary);
    }

    [HttpGet("summary/export")]
    public async Task<IActionResult> ExportSummaryCsv([FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc, CancellationToken cancellationToken)
    {
        var csv = await complianceReportingService.ExportSummaryCsvAsync(fromUtc, toUtc, cancellationToken);
        var fileName = $"compliance-summary-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
    }
}