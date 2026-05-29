using System.Text;
using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/v1/admin/audit-logs")]
public class AuditLogsController(IIdentityAuditReadService auditReadService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string action,
        [FromQuery] string outcome,
        [FromQuery] string subjectId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var response = await auditReadService.QueryAsync(new IdentityAuditLogQueryRequest
        {
            Action = action,
            Outcome = outcome,
            SubjectId = subjectId,
            FromUtc = fromUtc,
            ToUtc = toUtc,
            Page = page,
            PageSize = pageSize
        }, cancellationToken);

        return Ok(response);
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportCsv(
        [FromQuery] string action,
        [FromQuery] string outcome,
        [FromQuery] string subjectId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        CancellationToken cancellationToken = default)
    {
        var csv = await auditReadService.ExportCsvAsync(new IdentityAuditLogQueryRequest
        {
            Action = action,
            Outcome = outcome,
            SubjectId = subjectId,
            FromUtc = fromUtc,
            ToUtc = toUtc,
            Page = 1,
            PageSize = 5000
        }, cancellationToken);

        var fileName = $"identity-audit-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
    }
}
