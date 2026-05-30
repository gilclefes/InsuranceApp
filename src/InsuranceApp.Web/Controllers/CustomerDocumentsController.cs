using InsuranceApp.Infrastructure.Persistence;
using InsuranceApp.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace InsuranceApp.Web.Controllers;

[Authorize(Roles = "Admin,Agent,OfficeStaff")]
public class CustomerDocumentsController(InsuranceDbContext dbContext) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? customerNumber, string? documentType, CancellationToken cancellationToken)
    {
        var viewModel = new CustomerDocumentsViewModel
        {
            CustomerNumberFilter = customerNumber ?? string.Empty,
            DocumentTypeFilter = documentType ?? string.Empty,
            Documents = Array.Empty<CustomerDocumentListItem>()
        };

        try
        {
            var query = dbContext.CustomerDocuments
                .AsNoTracking()
                .Include(x => x.Customer)
                .Include(x => x.Policy)
                .Include(x => x.Claim)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(customerNumber))
            {
                var normalized = customerNumber.Trim().ToUpperInvariant();
                query = query.Where(x => x.Customer != null && x.Customer.CustomerNumber == normalized);
            }

            if (!string.IsNullOrWhiteSpace(documentType))
            {
                var normalizedType = documentType.Trim().ToLowerInvariant();
                query = query.Where(x => x.DocumentType.ToLower() == normalizedType);
            }

            viewModel.Documents = await query
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(200)
                .Select(x => new CustomerDocumentListItem
                {
                    DocumentReference = x.DocumentReference,
                    DocumentType = x.DocumentType,
                    CustomerNumber = x.Customer != null ? x.Customer.CustomerNumber : string.Empty,
                    CustomerName = x.Customer != null ? (x.Customer.FirstName + " " + x.Customer.LastName).Trim() : string.Empty,
                    FileName = x.FileName,
                    ContentType = x.ContentType,
                    StorageProvider = x.StorageProvider,
                    StorageUrl = x.StorageUrl,
                    UploadedByUserId = x.UploadedByUserId,
                    CreatedAtUtc = x.CreatedAtUtc,
                    PolicyNumber = x.Policy != null ? x.Policy.PolicyNumber : string.Empty,
                    ClaimNumber = x.Claim != null ? x.Claim.ClaimNumber : string.Empty
                })
                .ToListAsync(cancellationToken);
        }
        catch (DbException)
        {
            TempData["Error"] = "Customer document registry table is not available yet. Run database migrations to enable this page.";
        }

        return View(viewModel);
    }
}
