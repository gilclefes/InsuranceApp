using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Payouts;
using InsuranceApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Web.Controllers;

[Authorize(Roles = "Admin")]
public class PayoutAdminController(IPayoutService payoutService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? status, string? claimNumber, CancellationToken cancellationToken)
    {
        var payouts = await payoutService.ListPayoutsAsync(status, claimNumber, null, null, cancellationToken);
        return View(new PayoutAdminViewModel
        {
            StatusFilter = status,
            ClaimFilter = claimNumber,
            Payouts = payouts
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Initiate(PayoutAdminViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.Payouts = await payoutService.ListPayoutsAsync(model.StatusFilter, model.ClaimFilter, null, null, cancellationToken);
            return View("Index", model);
        }

        try
        {
            var result = await payoutService.InitiatePayoutAsync(new InitiatePayoutRequest
            {
                ClaimNumber = model.ClaimNumber,
                Amount = model.Amount,
                DestinationChannel = model.DestinationChannel,
                DestinationAccount = model.DestinationAccount,
                ExternalReference = model.ExternalReference
            }, cancellationToken);

            TempData["Success"] = $"Payout initiated: {result.PayoutReference}";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reconcile(CancellationToken cancellationToken)
    {
        var result = await payoutService.RunReconciliationAsync(cancellationToken);
        TempData["Success"] = $"Reconciliation run complete. Success: {result.ReconciledCount}, Failed: {result.FailedCount}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessWebhook(PayoutAdminViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            var eventId = string.IsNullOrWhiteSpace(model.WebhookEventId)
                ? $"POEV-{Guid.NewGuid():N}"[..18]
                : model.WebhookEventId.Trim();

            var result = await payoutService.ProcessWebhookAsync(new PayoutWebhookRequest
            {
                Provider = "MOCK_PAYOUT_PROVIDER",
                EventId = eventId,
                PayoutReference = model.WebhookPayoutReference,
                Status = model.WebhookStatus,
                Signature = "WEBHOOK-SIGNATURE",
                EventTimeUtc = DateTime.UtcNow,
                RawPayload = string.Empty
            }, cancellationToken);

            TempData["Success"] = result.Message;
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}