using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Claims;
using InsuranceApp.Infrastructure.Persistence;
using InsuranceApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Web.Controllers;

[Authorize(Roles = "Admin,Agent,OfficeStaff")]
public class ClaimsAdminController(
    IClaimService claimService,
    IObjectStorage objectStorage,
    InsuranceDbContext dbContext,
    ILogger<ClaimsAdminController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? status, string? adjusterUserId, string? policyNumber, string? timelineClaimNumber, CancellationToken cancellationToken)
    {
        var claims = await claimService.ListClaimsAsync(status, adjusterUserId, policyNumber, cancellationToken);
        var dashboard = await claimService.GetSlaDashboardAsync(adjusterUserId, cancellationToken);
        var timeline = Array.Empty<ClaimTimelineEventResponse>();
        var adjusters = await LoadAdjusterOptionsAsync(cancellationToken);
        var adjusterDisplayById = adjusters.ToDictionary(x => x.Value, x => x.Label);

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
            Dashboard = dashboard,
            AdjusterOptions = adjusters,
            AdjusterDisplayById = adjusterDisplayById
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClaimsAdminViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.Claims = await claimService.ListClaimsAsync(model.StatusFilter, model.AdjusterFilter, model.PolicyFilter, cancellationToken);
            model.AdjusterOptions = await LoadAdjusterOptionsAsync(cancellationToken);
            model.AdjusterDisplayById = model.AdjusterOptions.ToDictionary(x => x.Value, x => x.Label);
            return View("Index", model);
        }

        try
        {
            var evidenceUrl = model.EvidenceUrl?.Trim() ?? string.Empty;
            IFormFile? evidenceFile = null;
            if (model.EvidenceDocument is { Length: > 0 })
            {
                if (model.EvidenceDocument.Length > 10 * 1024 * 1024)
                {
                    TempData["Error"] = "Evidence document exceeds 10 MB limit.";
                    return RedirectToAction(nameof(Index));
                }

                evidenceFile = model.EvidenceDocument;
                evidenceUrl = await UploadEvidenceAsync(model.PolicyNumber, model.EvidenceDocument, cancellationToken);
            }

            var response = await claimService.CreateClaimAsync(new CreateClaimRequest
            {
                PolicyNumber = model.PolicyNumber,
                IncidentDate = model.IncidentDate,
                ClaimType = model.ClaimType,
                ClaimedAmount = model.ClaimedAmount,
                EvidenceUrl = evidenceUrl
            }, cancellationToken);

            if (evidenceFile is not null)
            {
                var claim = await dbContext.Claims
                    .AsNoTracking()
                    .Include(x => x.Policy)
                    .SingleOrDefaultAsync(x => x.ClaimNumber == response.ClaimNumber, cancellationToken);

                if (claim?.Policy is not null)
                {
                    dbContext.CustomerDocuments.Add(new InsuranceApp.Domain.Entities.CustomerDocument
                    {
                        CustomerId = claim.Policy.CustomerId,
                        PolicyId = claim.PolicyId,
                        ClaimId = claim.Id,
                        DocumentReference = $"CDOC-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8]}",
                        DocumentType = "ClaimEvidence",
                        FileName = Path.GetFileName(evidenceFile.FileName),
                        ContentType = string.IsNullOrWhiteSpace(evidenceFile.ContentType) ? "application/octet-stream" : evidenceFile.ContentType,
                        StorageProvider = objectStorage.Provider,
                        StorageUrl = evidenceUrl,
                        UploadedByUserId = User.Identity?.Name ?? string.Empty,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    });

                    try
                    {
                        await dbContext.SaveChangesAsync(cancellationToken);
                    }
                    catch (DbUpdateException)
                    {
                        logger.LogWarning("Customer document registry is unavailable; claim evidence metadata was not indexed.");
                    }
                }
            }

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

    private async Task<string> UploadEvidenceAsync(string policyNumber, IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken);

        var safeName = Path.GetFileName(file.FileName);
        var normalizedPolicy = string.IsNullOrWhiteSpace(policyNumber) ? "unknown" : policyNumber.Trim().ToUpperInvariant();
        var key = $"claims/{normalizedPolicy}/{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}-{safeName}";
        var contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;
        var url = await objectStorage.UploadAsync(new ObjectStorageUploadRequest(key, ms.ToArray(), contentType), cancellationToken);
        logger.LogInformation("Uploaded claim evidence {FileName} to {Url}", safeName, url);
        return url;
    }

    private async Task<IReadOnlyCollection<LookupOption>> LoadAdjusterOptionsAsync(CancellationToken cancellationToken)
    {
        var agentRoleIds = await dbContext.Roles
            .AsNoTracking()
            .Where(x => x.NormalizedName == "AGENT")
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var agents = await (
            from user in dbContext.Users.AsNoTracking()
            join userRole in dbContext.UserRoles.AsNoTracking() on user.Id equals userRole.UserId
            where agentRoleIds.Contains(userRole.RoleId)
            orderby user.Email
            select new LookupOption
            {
                Value = user.Id,
                Label = $"{user.Email} ({user.Id})"
            })
        .ToListAsync(cancellationToken);

        return agents;
    }
}