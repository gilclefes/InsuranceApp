using System.Text;
using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Audit;
using InsuranceApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Web.Controllers;

[Authorize(Roles = "Admin")]
public class ReportsController(
    IFinanceReportingService financeReportingService,
    IIdentityAuditReadService auditReadService) : Controller
{
    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(Finance));

    [HttpGet]
    public async Task<IActionResult> Finance(CancellationToken cancellationToken)
    {
        var report = await financeReportingService.GetFinanceReportAsync(cancellationToken);
        return View(new ReportsFinanceViewModel { Report = report });
    }

    [HttpGet]
    public async Task<IActionResult> AuditLog(string? action, string? outcome, string? subjectId, DateTime? fromUtc, DateTime? toUtc, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var query = new IdentityAuditLogQueryRequest
        {
            Action = action ?? string.Empty,
            Outcome = outcome ?? string.Empty,
            SubjectId = subjectId ?? string.Empty,
            FromUtc = fromUtc,
            ToUtc = toUtc,
            Page = page < 1 ? 1 : page,
            PageSize = pageSize < 1 ? 50 : Math.Min(pageSize, 500)
        };

        var result = await auditReadService.QueryAsync(query, cancellationToken);
        return View(new ReportsAuditViewModel { Query = query, Result = result });
    }

    [HttpGet]
    public async Task<IActionResult> AuditLogExport(string? action, string? outcome, string? subjectId, DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken)
    {
        var query = new IdentityAuditLogQueryRequest
        {
            Action = action ?? string.Empty,
            Outcome = outcome ?? string.Empty,
            SubjectId = subjectId ?? string.Empty,
            FromUtc = fromUtc,
            ToUtc = toUtc,
            Page = 1,
            PageSize = 10000
        };

        var csv = await auditReadService.ExportCsvAsync(query, cancellationToken);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"identity-audit-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }
}
