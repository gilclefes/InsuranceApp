using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.PremiumCollections;
using InsuranceApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Web.Controllers;

[Authorize(Roles = "Admin")]
public class PremiumAdminController(IPremiumCollectionService premiumCollectionService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? policyNumber, CancellationToken cancellationToken)
    {
        var model = new PremiumAdminViewModel
        {
            PolicyNumber = policyNumber ?? string.Empty
        };

        if (!string.IsNullOrWhiteSpace(policyNumber))
        {
            model.Items = await premiumCollectionService.ListPolicyCollectionsAsync(policyNumber, cancellationToken);
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateMandate(PremiumAdminViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.Items = await premiumCollectionService.ListPolicyCollectionsAsync(model.PolicyNumber, cancellationToken);
            return View("Index", model);
        }

        try
        {
            await premiumCollectionService.CreateMandateAsync(new CreatePremiumMandateRequest
            {
                PolicyNumber = model.PolicyNumber,
                Provider = model.Provider,
                PaymentChannel = model.PaymentChannel,
                ExternalReference = $"WEB-{Guid.NewGuid():N}"[..16],
                FirstDueDateUtc = model.DueDateUtc
            }, cancellationToken);

            TempData["Success"] = "Premium mandate captured.";
            return RedirectToAction(nameof(Index), new { policyNumber = model.PolicyNumber });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index), new { policyNumber = model.PolicyNumber });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Schedule(PremiumAdminViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.Items = await premiumCollectionService.ListPolicyCollectionsAsync(model.PolicyNumber, cancellationToken);
            return View("Index", model);
        }

        try
        {
            await premiumCollectionService.ScheduleCollectionAsync(model.PolicyNumber, new SchedulePremiumCollectionRequest
            {
                DueDateUtc = model.DueDateUtc,
                Amount = model.Amount,
                Provider = model.Provider,
                PaymentChannel = model.PaymentChannel
            }, cancellationToken);

            TempData["Success"] = "Premium collection schedule created.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { policyNumber = model.PolicyNumber });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RunDue(CancellationToken cancellationToken)
    {
        var result = await premiumCollectionService.RunDueCollectionsAsync(cancellationToken);
        TempData["Success"] = $"Run complete. Succeeded: {result.ItemsSucceeded}, Failed: {result.ItemsFailed}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RetryFailed(CancellationToken cancellationToken)
    {
        var result = await premiumCollectionService.RetryFailedCollectionsAsync(cancellationToken);
        TempData["Success"] = $"Retry complete. Recovered: {result.ItemsSucceeded}.";
        return RedirectToAction(nameof(Index));
    }
}