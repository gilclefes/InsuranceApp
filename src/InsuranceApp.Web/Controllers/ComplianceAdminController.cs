using System.Text;
using InsuranceApp.Application.Interfaces;
using InsuranceApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Web.Controllers;

[Authorize(Roles = "Admin")]
public class ComplianceAdminController(IComplianceReportingService complianceReportingService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken)
    {
        var summary = await complianceReportingService.GetSummaryAsync(fromUtc, toUtc, cancellationToken);

        return View(new ComplianceAdminViewModel
        {
            FromUtc = fromUtc,
            ToUtc = toUtc,
            Summary = summary
        });
    }

    [HttpGet]
    public async Task<IActionResult> Export(DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken)
    {
        var csv = await complianceReportingService.ExportSummaryCsvAsync(fromUtc, toUtc, cancellationToken);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"compliance-summary-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }
}