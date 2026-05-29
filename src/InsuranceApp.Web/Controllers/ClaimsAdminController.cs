using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Claims;
using InsuranceApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Web.Controllers;

[Authorize(Roles = "Admin,Agent")]
public class ClaimsAdminController(IClaimService claimService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? status, string? adjusterUserId, string? policyNumber, string? timelineClaimNumber, CancellationToken cancellationToken)
    {
        var claims = await claimService.ListClaimsAsync(status, adjusterUserId, policyNumber, cancellationToken);
        var dashboard = await claimService.GetSlaDashboardAsync(adjusterUserId, cancellationToken);
        var timeline = Array.Empty<ClaimTimelineEventResponse>();

        if (!string.IsNullOrWhiteSpace(timelineClaimNumber))
        {
            try
            {
                timeline = (await claimService.GetTimelineAsync(timelineClaimNumber, cancellationToken)).ToArray();
            }
            catch (InvalidOperationException)
            {
                TempData["Error"] = "Timeline claim not found.";
            }
        }

        return View(new ClaimsAdminViewModel
        {
            StatusFilter = status,
            AdjusterFilter = adjusterUserId,
            PolicyFilter = policyNumber,
            TimelineClaimNumber = timelineClaimNumber ?? string.Empty,
            Claims = claims,
            Timeline = timeline,
            Dashboard = dashboard
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClaimsAdminViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.Claims = await claimService.ListClaimsAsync(model.StatusFilter, model.AdjusterFilter, model.PolicyFilter, cancellationToken);
            return View("Index", model);
        }

        try
        {
            var response = await claimService.CreateClaimAsync(new CreateClaimRequest
            {
                PolicyNumber = model.PolicyNumber,
                IncidentDate = model.IncidentDate,
                ClaimType = model.ClaimType,
                ClaimedAmount = model.ClaimedAmount,
                EvidenceUrl = model.EvidenceUrl
            }, cancellationToken);

            TempData["Success"] = $"Claim created: {response.ClaimNumber}";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(ClaimsAdminViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            await claimService.AssignClaimAsync(model.AssignClaimNumber, new AssignClaimRequest
            {
                AdjusterUserId = model.AssignAdjusterUserId,
                Note = model.AssignNote
            }, cancellationToken);

            TempData["Success"] = $"Claim {model.AssignClaimNumber} assigned.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(ClaimsAdminViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            await claimService.ReviewClaimAsync(model.ReviewClaimNumber, new ReviewClaimRequest
            {
                Action = model.ReviewAction,
                ApprovedAmount = model.ReviewApprovedAmount,
                Reason = model.ReviewReason
            }, cancellationToken);

            TempData["Success"] = $"Claim {model.ReviewClaimNumber} reviewed.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}