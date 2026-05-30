using System.Security.Claims;
using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Auth;
using InsuranceApp.Contracts.Onboarding;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Infrastructure.Persistence;
using InsuranceApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Web.Controllers;

[Authorize(Roles = "Agent,Admin,OfficeStaff")]
public class AgentController(
    IAgentPortalService agentPortalService,
    IOnboardingService onboardingService,
    IObjectStorage objectStorage,
    InsuranceDbContext dbContext,
    ILogger<AgentController> logger) : Controller
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

            if (model.CustomerIntakeForm is { Length: > 0 })
            {
                if (model.CustomerIntakeForm.Length > 10 * 1024 * 1024)
                {
                    TempData["Error"] = "Onboarding form exceeds 10 MB limit.";
                    return View(model);
                }

                var intakeDocumentUrl = await UploadFileAsync(
                    model.CustomerIntakeForm,
                    $"customer-intake/{profile.CustomerNumber}",
                    cancellationToken);

                dbContext.CustomerDocuments.Add(new CustomerDocument
                {
                    CustomerId = profile.CustomerId,
                    DocumentReference = $"CDOC-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8]}",
                    DocumentType = "IntakeForm",
                    FileName = Path.GetFileName(model.CustomerIntakeForm.FileName),
                    ContentType = string.IsNullOrWhiteSpace(model.CustomerIntakeForm.ContentType)
                        ? "application/octet-stream"
                        : model.CustomerIntakeForm.ContentType,
                    StorageProvider = objectStorage.Provider,
                    StorageUrl = intakeDocumentUrl,
                    UploadedByUserId = User.Identity?.Name ?? CurrentAgentId,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow
                });

                try
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateException)
                {
                    logger.LogWarning("Customer document registry is unavailable; onboarding form upload metadata was not indexed.");
                }

                TempData["Info"] = $"Intake form uploaded: {intakeDocumentUrl}";
            }

            TempData["Success"] = $"Customer {profile.CustomerNumber} onboarded.";
            return RedirectToAction(nameof(Portfolio));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return View(model);
        }
    }

    private async Task<string> UploadFileAsync(IFormFile file, string folder, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken);

        var safeName = Path.GetFileName(file.FileName);
        var key = $"{folder}/{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}-{safeName}";
        var contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;
        var url = await objectStorage.UploadAsync(new ObjectStorageUploadRequest(key, ms.ToArray(), contentType), cancellationToken);
        logger.LogInformation("Uploaded onboarding file {FileName} to {Url}", safeName, url);
        return url;
    }
}
