using System.Security.Claims;
using System.Text;
using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Claims;
using InsuranceApp.Contracts.Policies;
using InsuranceApp.Contracts.PremiumCollections;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Persistence;
using InsuranceApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Web.Controllers;

[Authorize(Roles = "Customer,Admin,Agent")]
public class CustomerController(
    InsuranceDbContext dbContext,
    IPolicyIssuanceService policyIssuanceService,
    IClaimService claimService,
    IPremiumCollectionService premiumCollectionService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var customer = await dbContext.Customers
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Email == email.ToLowerInvariant(), cancellationToken);

        if (customer is null)
        {
            return View(new CustomerDashboardViewModel
            {
                CustomerName = User.Identity?.Name ?? string.Empty,
                ProfileComplete = false
            });
        }

        var policies = await policyIssuanceService.GetDashboardAsync(customer.Id, null, null, cancellationToken);
        var claims = await dbContext.Claims
            .AsNoTracking()
            .Where(x => x.Policy != null && x.Policy.CustomerId == customer.Id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(5)
            .Select(c => new ClaimResponse
            {
                ClaimNumber = c.ClaimNumber,
                PolicyNumber = c.Policy!.PolicyNumber,
                IncidentDate = c.IncidentDate,
                ClaimType = c.ClaimType,
                ClaimedAmount = c.ClaimedAmount,
                ApprovedAmount = c.ApprovedAmount,
                Status = c.Status.ToString(),
                CreatedAtUtc = c.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        var outstanding = await dbContext.PremiumTransactions
            .AsNoTracking()
            .Where(x => x.Policy != null && x.Policy.CustomerId == customer.Id
                && (x.Status == PaymentStatus.Pending || x.Status == PaymentStatus.Failed))
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        return View(new CustomerDashboardViewModel
        {
            CustomerName = $"{customer.FirstName} {customer.LastName}".Trim(),
            ProfileComplete = true,
            KycVerified = customer.KycVerified,
            ActivePolicies = policies.Count(p => string.Equals(p.Status, "Active", StringComparison.OrdinalIgnoreCase)),
            OpenClaims = claims.Count(c => !string.Equals(c.Status, "Closed", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(c.Status, "Disbursed", StringComparison.OrdinalIgnoreCase)),
            OutstandingPremium = outstanding,
            RecentPolicies = policies.Take(5).ToList(),
            RecentClaims = claims
        });
    }

    [HttpGet]
    public async Task<IActionResult> Policies(CancellationToken cancellationToken)
    {
        var customer = await ResolveCustomerAsync(cancellationToken);
        if (customer is null)
        {
            return RedirectToAction(nameof(Dashboard));
        }

        var policies = await policyIssuanceService.GetDashboardAsync(customer.Id, null, null, cancellationToken);
        return View(new MyPoliciesViewModel { Policies = policies });
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmPolicy(string reference, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            return RedirectToAction("Index", "Products");
        }

        var quote = await dbContext.QuoteRecords
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.QuoteReference == reference, cancellationToken);
        if (quote is null)
        {
            return NotFound();
        }

        var product = await dbContext.ProductDefinitions
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.ProductCode == quote.ProductCode && x.IsActive, cancellationToken);

        return View(new ConfirmPolicyViewModel
        {
            QuoteReference = quote.QuoteReference,
            ProductCode = quote.ProductCode,
            ProductName = product?.Name ?? quote.ProductCode,
            TotalPremium = quote.TotalPremium,
            CurrencyCode = quote.CurrencyCode
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPolicy(ConfirmPolicyViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var customer = await ResolveCustomerAsync(cancellationToken);
        if (customer is null)
        {
            TempData["Error"] = "Complete your profile before purchasing a policy.";
            return RedirectToAction(nameof(Dashboard));
        }

        try
        {
            var response = await policyIssuanceService.IssueFromQuoteAsync(new IssuePolicyFromQuoteRequest
            {
                QuoteReference = model.QuoteReference,
                CustomerId = customer.Id,
                AssignedAgentId = string.Empty,
                CoverageType = model.CoverageType,
                InceptionDate = model.InceptionDate,
                ExpiryDate = model.ExpiryDate
            }, cancellationToken);

            // Persist signature as a policy document for audit (FR-008).
            var policy = await dbContext.Policies
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.PolicyNumber == response.PolicyNumber, cancellationToken);
            if (policy is not null)
            {
                dbContext.PolicyDocuments.Add(new PolicyDocument
                {
                    PolicyId = policy.Id,
                    DocumentReference = $"SIG-{response.PolicyNumber}",
                    DocumentType = "Signature",
                    FileName = $"signature-{response.PolicyNumber}.txt",
                    ContentType = "text/plain",
                    Content = Encoding.UTF8.GetBytes($"Signed by: {model.DigitalSignature}\nUTC: {DateTime.UtcNow:o}\nIP: {HttpContext.Connection.RemoteIpAddress}"),
                    Status = "Active"
                });

                // Create initial premium mandate per chosen channel.
                await premiumCollectionService.CreateMandateAsync(new CreatePremiumMandateRequest
                {
                    PolicyNumber = response.PolicyNumber,
                    Provider = model.PaymentChannel switch
                    {
                        "MoMo" => "MTN_MOMO",
                        "Bank" => "GHIPSS_ACH",
                        _ => "MANUAL"
                    },
                    PaymentChannel = model.PaymentChannel,
                    ExternalReference = model.MandateReference,
                    FirstDueDateUtc = model.InceptionDate
                }, cancellationToken);

                await dbContext.SaveChangesAsync(cancellationToken);
            }

            TempData["Success"] = $"Policy {response.PolicyNumber} issued. Premium {response.PremiumAmount:N2} confirmed.";
            return RedirectToAction(nameof(Policies));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> PolicyDocument(string policyNumber, CancellationToken cancellationToken)
    {
        var customer = await ResolveCustomerAsync(cancellationToken);
        if (customer is null) return Forbid();

        var normalizedPolicy = policyNumber.Trim().ToUpperInvariant();
        var owns = await dbContext.Policies
            .AsNoTracking()
            .AnyAsync(x => x.PolicyNumber == normalizedPolicy && x.CustomerId == customer.Id, cancellationToken);
        if (!owns) return Forbid();

        try
        {
            var file = await policyIssuanceService.GetPolicyDocumentAsync(normalizedPolicy, cancellationToken);
            return File(file.Content, file.ContentType, file.FileName);
        }
        catch (InvalidOperationException)
        {
            TempData["Error"] = "Document is not yet available for this policy.";
            return RedirectToAction(nameof(Policies));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Claims(CancellationToken cancellationToken)
    {
        var customer = await ResolveCustomerAsync(cancellationToken);
        if (customer is null)
        {
            return RedirectToAction(nameof(Dashboard));
        }

        var claims = await dbContext.Claims
            .AsNoTracking()
            .Where(x => x.Policy != null && x.Policy.CustomerId == customer.Id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(c => new ClaimResponse
            {
                ClaimNumber = c.ClaimNumber,
                PolicyNumber = c.Policy!.PolicyNumber,
                IncidentDate = c.IncidentDate,
                ClaimType = c.ClaimType,
                ClaimedAmount = c.ClaimedAmount,
                ApprovedAmount = c.ApprovedAmount,
                Status = c.Status.ToString(),
                CreatedAtUtc = c.CreatedAtUtc,
                DecisionedAtUtc = c.DecisionedAtUtc
            })
            .ToListAsync(cancellationToken);

        return View(new MyClaimsViewModel { Claims = claims });
    }

    [HttpGet]
    public async Task<IActionResult> NewClaim(CancellationToken cancellationToken)
    {
        var customer = await ResolveCustomerAsync(cancellationToken);
        if (customer is null) return RedirectToAction(nameof(Dashboard));

        var eligible = (await policyIssuanceService.GetDashboardAsync(customer.Id, null, "Active", cancellationToken)).ToList();
        return View(new NewClaimViewModel { EligiblePolicies = eligible });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NewClaim(NewClaimViewModel model, CancellationToken cancellationToken)
    {
        var customer = await ResolveCustomerAsync(cancellationToken);
        if (customer is null) return RedirectToAction(nameof(Dashboard));

        if (!ModelState.IsValid)
        {
            model.EligiblePolicies = (await policyIssuanceService.GetDashboardAsync(customer.Id, null, "Active", cancellationToken)).ToList();
            return View(model);
        }

        var normalizedPolicy = model.PolicyNumber.Trim().ToUpperInvariant();
        var owns = await dbContext.Policies
            .AsNoTracking()
            .AnyAsync(x => x.PolicyNumber == normalizedPolicy && x.CustomerId == customer.Id, cancellationToken);
        if (!owns)
        {
            ModelState.AddModelError(string.Empty, "Selected policy does not belong to your account.");
            model.EligiblePolicies = (await policyIssuanceService.GetDashboardAsync(customer.Id, null, "Active", cancellationToken)).ToList();
            return View(model);
        }

        try
        {
            var claim = await claimService.CreateClaimAsync(new CreateClaimRequest
            {
                PolicyNumber = normalizedPolicy,
                IncidentDate = model.IncidentDate,
                ClaimType = model.ClaimType,
                ClaimedAmount = model.ClaimedAmount,
                EvidenceUrl = model.EvidenceUrl
            }, cancellationToken);

            TempData["Success"] = $"Claim {claim.ClaimNumber} submitted. We will notify you with updates.";
            return RedirectToAction(nameof(Claims));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.EligiblePolicies = (await policyIssuanceService.GetDashboardAsync(customer.Id, null, "Active", cancellationToken)).ToList();
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Payments(CancellationToken cancellationToken)
    {
        var customer = await ResolveCustomerAsync(cancellationToken);
        if (customer is null) return RedirectToAction(nameof(Dashboard));

        var payments = await dbContext.PremiumTransactions
            .AsNoTracking()
            .Where(x => x.Policy != null && x.Policy.CustomerId == customer.Id)
            .OrderByDescending(x => x.DueDateUtc)
            .Select(x => new PremiumCollectionItemResponse
            {
                TransactionReference = x.TransactionReference,
                PolicyNumber = x.Policy!.PolicyNumber,
                Amount = x.Amount,
                CurrencyCode = x.Policy.CurrencyCode,
                Provider = x.Provider,
                PaymentChannel = x.PaymentChannel,
                Status = x.Status.ToString(),
                DueDateUtc = x.DueDateUtc,
                ProcessedAtUtc = x.ProcessedAtUtc,
                RetryCount = x.RetryCount
            })
            .ToListAsync(cancellationToken);

        return View(new MyPaymentsViewModel { Payments = payments });
    }

    [HttpGet]
    public async Task<IActionResult> Notifications(CancellationToken cancellationToken)
    {
        var customer = await ResolveCustomerAsync(cancellationToken);
        if (customer is null) return RedirectToAction(nameof(Dashboard));

        var policyNotifications = await dbContext.PolicyNotifications
            .AsNoTracking()
            .Where(x => x.Policy != null && x.Policy.CustomerId == customer.Id)
            .OrderByDescending(x => x.SentAtUtc)
            .Take(50)
            .ToListAsync(cancellationToken);

        var claimNotifications = await dbContext.ClaimNotifications
            .AsNoTracking()
            .Where(x => x.Claim != null && x.Claim.Policy != null && x.Claim.Policy.CustomerId == customer.Id)
            .OrderByDescending(x => x.SentAtUtc)
            .Take(50)
            .ToListAsync(cancellationToken);

        return View(new MyNotificationsViewModel
        {
            PolicyNotifications = policyNotifications,
            ClaimNotifications = claimNotifications
        });
    }

    private async Task<Customer?> ResolveCustomerAsync(CancellationToken cancellationToken)
    {
        var email = (User.FindFirstValue(ClaimTypes.Email) ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }
        return await dbContext.Customers
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
    }
}
