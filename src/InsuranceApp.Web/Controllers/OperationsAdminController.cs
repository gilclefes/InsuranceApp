using InsuranceApp.Application.Interfaces;
using InsuranceApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Web.Controllers;

[Authorize(Roles = "Admin")]
public class OperationsAdminController(IOperationsHardeningService operationsHardeningService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var readiness = await operationsHardeningService.GetReadinessSummaryAsync(cancellationToken);
        return View(new OperationsAdminViewModel { Readiness = readiness });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RunDrill(CancellationToken cancellationToken)
    {
        var readiness = await operationsHardeningService.GetReadinessSummaryAsync(cancellationToken);
        var drill = await operationsHardeningService.RunFailoverDrillAsync(cancellationToken);

        TempData["Success"] = drill.Successful ? "Failover drill completed successfully." : "Failover drill completed with one or more failed steps.";

        return View("Index", new OperationsAdminViewModel
        {
            Readiness = readiness,
            LastDrillResult = drill
        });
    }
}