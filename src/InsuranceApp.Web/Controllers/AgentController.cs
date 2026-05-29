using System.Security.Claims;
using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Auth;
using InsuranceApp.Contracts.Onboarding;
using InsuranceApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Web.Controllers;

[Authorize(Roles = "Agent,Admin")]
public class AgentController(
    IAgentPortalService agentPortalService,
    IOnboardingService onboardingService) : Controller
{
    private string CurrentAgentId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(Portfolio));

    [HttpGet]
    public async Task<IActionResult> Portfolio(CancellationToken cancellationToken)
    {
        var portfolio = await agentPortalService.GetPortfolioAsync(CurrentAgentId, cancellationToken);
        return View(new AgentPortfolioViewModel { Portfolio = portfolio });
    }

    [HttpGet]
    public async Task<IActionResult> Commission(CancellationToken cancellationToken)
    {
        var ledger = await agentPortalService.GetCommissionLedgerAsync(CurrentAgentId, cancellationToken);
        return View(new AgentCommissionViewModel { Ledger = ledger });
    }

    [HttpGet]
    public IActionResult Onboard() => View(new AgentOnboardViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Onboard(AgentOnboardViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var context = new AuthRequestContext
            {
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                UserAgent = Request.Headers.UserAgent.ToString(),
                DeviceId = "agent-web"
            };

            var profile = await onboardingService.AgentOnboardAsync(model.Request, context, CurrentAgentId, cancellationToken);
            TempData["Success"] = $"Customer {profile.CustomerNumber} onboarded.";
            return RedirectToAction(nameof(Portfolio));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return View(model);
        }
    }
}
