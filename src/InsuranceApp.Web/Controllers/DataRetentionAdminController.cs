using InsuranceApp.Application.Interfaces;
using InsuranceApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Web.Controllers;

[Authorize(Roles = "Admin")]
public class DataRetentionAdminController(IDataRetentionService dataRetentionService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var summary = await dataRetentionService.GetSummaryAsync(cancellationToken);
        return View(new DataRetentionAdminViewModel { Summary = summary });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Run(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid retention command.";
            return RedirectToAction(nameof(Index));
        }

        var result = await dataRetentionService.RunRetentionAsync(cancellationToken);
        TempData["Success"] =
            $"Retention completed. OTP deleted: {result.DeletedOtpChallenges}, Tokens deleted: {result.DeletedRefreshTokens}, Audit anonymized: {result.AnonymizedIdentityAuditRows}.";

        return RedirectToAction(nameof(Index));
    }
}