using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Policies;
using InsuranceApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Web.Controllers;

[Authorize(Roles = "Admin,Agent")]
public class PolicyAdminController(IPolicyIssuanceService policyIssuanceService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? status, CancellationToken cancellationToken)
    {
        var policies = await policyIssuanceService.GetDashboardAsync(null, null, status, cancellationToken);
        return View(new PolicyDashboardViewModel
        {
            StatusFilter = status,
            Policies = policies
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreatePolicyViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePolicyViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var response = await policyIssuanceService.IssueFromQuoteAsync(new IssuePolicyFromQuoteRequest
            {
                QuoteReference = model.QuoteReference,
                CustomerId = model.CustomerId,
                AssignedAgentId = model.AssignedAgentId,
                CoverageType = model.CoverageType,
                InceptionDate = model.InceptionDate,
                ExpiryDate = model.ExpiryDate
            }, cancellationToken);

            TempData["Success"] = $"Policy issued: {response.PolicyNumber}";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Endorse(string policyNumber, string note, CancellationToken cancellationToken)
    {
        try
        {
            await policyIssuanceService.EndorsePolicyAsync(policyNumber, new EndorsePolicyRequest
            {
                CoverageType = string.Empty,
                PremiumAdjustment = 0,
                Reason = note
            }, cancellationToken);
            TempData["Success"] = $"Policy {policyNumber} endorsed.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(string policyNumber, string reason, CancellationToken cancellationToken)
    {
        try
        {
            await policyIssuanceService.CancelPolicyAsync(policyNumber, new CancelPolicyRequest { Reason = reason }, cancellationToken);
            TempData["Success"] = $"Policy {policyNumber} cancelled.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Document(string policyNumber, CancellationToken cancellationToken)
    {
        try
        {
            var file = await policyIssuanceService.GetPolicyDocumentAsync(policyNumber, cancellationToken);
            return File(file.Content, file.ContentType, file.FileName);
        }
        catch (InvalidOperationException)
        {
            TempData["Error"] = "Document not found for policy.";
            return RedirectToAction(nameof(Index));
        }
    }
}