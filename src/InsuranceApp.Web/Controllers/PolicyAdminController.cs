using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Policies;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Infrastructure.Persistence;
using InsuranceApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Web.Controllers;

[Authorize(Roles = "Admin,Agent,OfficeStaff")]
public class PolicyAdminController(
    IPolicyIssuanceService policyIssuanceService,
    InsuranceDbContext dbContext,
    IObjectStorage objectStorage,
    ILogger<PolicyAdminController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? status, CancellationToken cancellationToken)
    {
        var policies = await policyIssuanceService.GetDashboardAsync(null, null, status, cancellationToken);

        var customerIds = policies.Select(x => x.CustomerId).Distinct().ToList();
        var customerDisplayById = await dbContext.Customers
            .AsNoTracking()
            .Where(x => customerIds.Contains(x.Id))
            .ToDictionaryAsync(
                x => x.Id,
                x => $"{x.CustomerNumber} - {x.FirstName} {x.LastName}".Trim(),
                cancellationToken);

        var agentIds = policies
            .Select(x => x.AssignedAgentId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        var agentDisplayById = await dbContext.Users
            .AsNoTracking()
            .Where(x => agentIds.Contains(x.Id))
            .ToDictionaryAsync(
                x => x.Id,
                x => string.IsNullOrWhiteSpace(x.Email) ? x.Id : x.Email,
                cancellationToken);

        return View(new PolicyDashboardViewModel
        {
            StatusFilter = status,
            Policies = policies,
            CustomerDisplayById = customerDisplayById,
            AgentDisplayById = agentDisplayById
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new CreatePolicyViewModel();
        await LoadCreateLookupsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePolicyViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadCreateLookupsAsync(model, cancellationToken);
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

            if (model.SupportingDocument is { Length: > 0 })
            {
                if (model.SupportingDocument.Length > 10 * 1024 * 1024)
                {
                    ModelState.AddModelError(nameof(model.SupportingDocument), "Document exceeds 10 MB limit.");
                    await LoadCreateLookupsAsync(model, cancellationToken);
                    return View(model);
                }

                await SavePolicyDocumentAsync(response.PolicyNumber, model.SupportingDocument, cancellationToken);
            }

            TempData["Success"] = $"Policy issued: {response.PolicyNumber}";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadCreateLookupsAsync(model, cancellationToken);
            return View(model);
        }
    }

    private async Task LoadCreateLookupsAsync(CreatePolicyViewModel model, CancellationToken cancellationToken)
    {
        var customers = await dbContext.Customers
            .AsNoTracking()
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .Select(x => new LookupOption
            {
                Value = x.Id.ToString(),
                Label = $"{x.CustomerNumber} - {x.FirstName} {x.LastName} ({x.Email})"
            })
            .ToListAsync(cancellationToken);

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

        var unassigned = new LookupOption { Value = string.Empty, Label = "-- Unassigned --" };
        model.CustomerOptions = customers;
        model.AgentOptions = new[] { unassigned }.Concat(agents).ToList();
    }

    private async Task SavePolicyDocumentAsync(string policyNumber, IFormFile file, CancellationToken cancellationToken)
    {
        var normalized = policyNumber.Trim().ToUpperInvariant();
        var policy = await dbContext.Policies.SingleOrDefaultAsync(x => x.PolicyNumber == normalized, cancellationToken);
        if (policy is null)
        {
            logger.LogWarning("Policy {PolicyNumber} not found while saving supporting document.", normalized);
            return;
        }

        await using var stream = file.OpenReadStream();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken);
        var fileBytes = ms.ToArray();
        var safeName = Path.GetFileName(file.FileName);
        var key = $"policies/{normalized}/{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}-{safeName}";
        var contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;
        var storageUrl = await objectStorage.UploadAsync(new ObjectStorageUploadRequest(key, fileBytes, contentType), cancellationToken);

        dbContext.PolicyDocuments.Add(new PolicyDocument
        {
            PolicyId = policy.Id,
            DocumentReference = $"PDOC-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8]}",
            DocumentType = "CustomerSupportingDocument",
            FileName = safeName,
            ContentType = contentType,
            Status = "Uploaded",
            Content = fileBytes,
            GeneratedAtUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.CustomerDocuments.Add(new CustomerDocument
        {
            CustomerId = policy.CustomerId,
            PolicyId = policy.Id,
            DocumentReference = $"CDOC-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8]}",
            DocumentType = "PolicySupportingDocument",
            FileName = safeName,
            ContentType = contentType,
            StorageProvider = objectStorage.Provider,
            StorageUrl = storageUrl,
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
            logger.LogWarning("Customer document registry is unavailable; policy supporting document metadata was not indexed.");
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