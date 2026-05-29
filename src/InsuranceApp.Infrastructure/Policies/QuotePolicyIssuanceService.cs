using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Policies;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InsuranceApp.Infrastructure.Policies;

public class QuotePolicyIssuanceService(InsuranceDbContext dbContext) : IPolicyIssuanceService
{
    public async Task<IssuePolicyFromQuoteResponse> IssueFromQuoteAsync(IssuePolicyFromQuoteRequest request, CancellationToken cancellationToken = default)
    {
        var quote = await dbContext.QuoteRecords
            .SingleOrDefaultAsync(x => x.QuoteReference == request.QuoteReference, cancellationToken)
            ?? throw new InvalidOperationException("Quote reference not found.");

        if (!string.Equals(quote.Status, "Quoted", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Quote has already been processed or is not available for issuance.");
        }

        if (quote.ValidUntilUtc < DateTime.UtcNow)
        {
            quote.Status = "Expired";
            await dbContext.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException("Quote has expired.");
        }

        var product = await dbContext.ProductDefinitions
            .SingleOrDefaultAsync(x => x.ProductCode == quote.ProductCode, cancellationToken)
            ?? throw new InvalidOperationException("Quote product is not configured.");

        var customer = await dbContext.Customers
            .SingleOrDefaultAsync(x => x.Id == request.CustomerId, cancellationToken)
            ?? throw new InvalidOperationException("Customer not found.");

        var policyNumber = $"POL-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6]}";

        var policy = new Policy
        {
            PolicyNumber = policyNumber,
            CustomerId = request.CustomerId,
            ProductType = product.ProductType,
            AssignedAgentId = request.AssignedAgentId,
            CoverageType = string.IsNullOrWhiteSpace(request.CoverageType) ? "Standard" : request.CoverageType,
            PremiumAmount = quote.TotalPremium,
            CurrencyCode = quote.CurrencyCode,
            InceptionDate = request.InceptionDate,
            ExpiryDate = request.ExpiryDate,
            Status = PolicyStatus.Active
        };

        dbContext.Policies.Add(policy);

        quote.Status = "Issued";
        quote.IssuedPolicyNumber = policyNumber;

        await dbContext.SaveChangesAsync(cancellationToken);

        var documentReference = $"DOC-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6]}";
        var document = new PolicyDocument
        {
            PolicyId = policy.Id,
            DocumentReference = documentReference,
            DocumentType = "PolicySchedule",
            FileName = $"{policyNumber}-schedule.pdf",
            ContentType = "application/pdf",
            Status = "Generated",
            Content = BuildPolicySchedulePdfBytes(policy, quote)
        };

        dbContext.PolicyDocuments.Add(document);
        await CreateNotificationsAsync(policy, customer, "PolicyIssued", cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new IssuePolicyFromQuoteResponse
        {
            PolicyNumber = policy.PolicyNumber,
            QuoteReference = quote.QuoteReference,
            Status = policy.Status.ToString(),
            PremiumAmount = policy.PremiumAmount,
            ProductCode = quote.ProductCode,
            DocumentReference = documentReference
        };
    }

    public async Task<PolicyDocumentResponse> GetPolicyDocumentAsync(string policyNumber, CancellationToken cancellationToken = default)
    {
        var policy = await dbContext.Policies
            .SingleOrDefaultAsync(x => x.PolicyNumber == policyNumber, cancellationToken)
            ?? throw new InvalidOperationException("Policy not found.");

        var document = await dbContext.PolicyDocuments
            .Where(x => x.PolicyId == policy.Id)
            .OrderByDescending(x => x.GeneratedAtUtc)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Policy document not found.");

        return new PolicyDocumentResponse
        {
            PolicyNumber = policy.PolicyNumber,
            DocumentReference = document.DocumentReference,
            FileName = document.FileName,
            ContentType = document.ContentType,
            Content = document.Content
        };
    }

    public async Task<IReadOnlyCollection<PolicySummaryResponse>> GetDashboardAsync(long? customerId, string? agentUserId, string? status, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Policies.AsNoTracking().AsQueryable();

        if (customerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == customerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(agentUserId))
        {
            query = query.Where(x => x.AssignedAgentId == agentUserId);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PolicyStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(x => x.Status == parsedStatus);
        }

        var policies = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var policyIds = policies.Select(x => x.Id).ToHashSet();
        var docs = await dbContext.PolicyDocuments
            .Where(x => policyIds.Contains(x.PolicyId))
            .GroupBy(x => x.PolicyId)
            .Select(x => new { PolicyId = x.Key, DocRef = x.OrderByDescending(d => d.GeneratedAtUtc).Select(d => d.DocumentReference).FirstOrDefault() })
            .ToDictionaryAsync(x => x.PolicyId, x => x.DocRef ?? string.Empty, cancellationToken);

        return policies.Select(x => new PolicySummaryResponse
        {
            PolicyNumber = x.PolicyNumber,
            CustomerId = x.CustomerId,
            AssignedAgentId = x.AssignedAgentId,
            ProductType = x.ProductType.ToString(),
            CoverageType = x.CoverageType,
            PremiumAmount = x.PremiumAmount,
            CurrencyCode = x.CurrencyCode,
            InceptionDate = x.InceptionDate,
            ExpiryDate = x.ExpiryDate,
            Status = x.Status.ToString(),
            DocumentReference = docs.TryGetValue(x.Id, out var docRef) ? docRef : string.Empty
        }).ToList();
    }

    public async Task<PolicyOperationResponse> EndorsePolicyAsync(string policyNumber, EndorsePolicyRequest request, CancellationToken cancellationToken = default)
    {
        var policy = await dbContext.Policies
            .SingleOrDefaultAsync(x => x.PolicyNumber == policyNumber, cancellationToken)
            ?? throw new InvalidOperationException("Policy not found.");

        if (policy.Status != PolicyStatus.Active)
        {
            throw new InvalidOperationException("Only active policies can be endorsed.");
        }

        if (!string.IsNullOrWhiteSpace(request.CoverageType))
        {
            policy.CoverageType = request.CoverageType;
        }

        policy.PremiumAmount = decimal.Round(policy.PremiumAmount + request.PremiumAdjustment, 2, MidpointRounding.AwayFromZero);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PolicyOperationResponse
        {
            PolicyNumber = policy.PolicyNumber,
            Status = policy.Status.ToString(),
            Message = "Policy endorsement applied."
        };
    }

    public async Task<PolicyOperationResponse> CancelPolicyAsync(string policyNumber, CancelPolicyRequest request, CancellationToken cancellationToken = default)
    {
        var policy = await dbContext.Policies
            .SingleOrDefaultAsync(x => x.PolicyNumber == policyNumber, cancellationToken)
            ?? throw new InvalidOperationException("Policy not found.");

        if (policy.Status == PolicyStatus.Cancelled)
        {
            throw new InvalidOperationException("Policy is already cancelled.");
        }

        policy.Status = PolicyStatus.Cancelled;
        policy.CancellationReason = string.IsNullOrWhiteSpace(request.Reason) ? "UserRequested" : request.Reason;
        policy.ExpiryDate = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PolicyOperationResponse
        {
            PolicyNumber = policy.PolicyNumber,
            Status = policy.Status.ToString(),
            Message = "Policy cancelled."
        };
    }

    public async Task<RenewalReminderRunResponse> RunRenewalReminderCycleAsync(CancellationToken cancellationToken = default)
    {
        var from = DateTime.UtcNow.Date;
        var to = DateTime.UtcNow.Date.AddDays(30);

        var policies = await dbContext.Policies
            .Where(x => x.Status == PolicyStatus.Active && x.ExpiryDate >= from && x.ExpiryDate <= to)
            .ToListAsync(cancellationToken);

        var sentCount = 0;
        foreach (var policy in policies)
        {
            var hasRecentReminder = await dbContext.PolicyNotifications.AnyAsync(
                x => x.PolicyId == policy.Id && x.TemplateKey == "RenewalReminder" && x.SentAtUtc >= DateTime.UtcNow.AddDays(-7),
                cancellationToken);

            if (hasRecentReminder)
            {
                continue;
            }

            var customer = await dbContext.Customers.SingleOrDefaultAsync(x => x.Id == policy.CustomerId, cancellationToken);
            if (customer is null)
            {
                continue;
            }

            await CreateNotificationsAsync(policy, customer, "RenewalReminder", cancellationToken);
            sentCount++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new RenewalReminderRunResponse
        {
            PoliciesScanned = policies.Count,
            RemindersSent = sentCount
        };
    }

    private static byte[] BuildPolicySchedulePdfBytes(Policy policy, QuoteRecord quote)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(t => t.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Text("InsuranceApp – Policy Schedule").SemiBold().FontSize(18).FontColor(Colors.Blue.Darken3);
                    col.Item().Text($"Issued: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC").FontSize(9).FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingVertical(15).Column(col =>
                {
                    col.Spacing(8);
                    col.Item().Text(t => { t.Span("Policy Number: ").SemiBold(); t.Span(policy.PolicyNumber); });
                    col.Item().Text(t => { t.Span("Product Code: ").SemiBold(); t.Span(quote.ProductCode); });
                    col.Item().Text(t => { t.Span("Coverage Type: ").SemiBold(); t.Span(policy.CoverageType); });
                    col.Item().Text(t => { t.Span("Premium: ").SemiBold(); t.Span($"{policy.CurrencyCode} {policy.PremiumAmount:F2}"); });
                    col.Item().Text(t => { t.Span("Inception: ").SemiBold(); t.Span($"{policy.InceptionDate:yyyy-MM-dd}"); });
                    col.Item().Text(t => { t.Span("Expiry: ").SemiBold(); t.Span($"{policy.ExpiryDate:yyyy-MM-dd}"); });
                    col.Item().PaddingTop(20).Text("Terms and Conditions").SemiBold();
                    col.Item().Text("This schedule must be read with the master policy wording. Premiums are payable per the agreed mandate. Cover lapses on non-payment beyond the grace period.").FontSize(9).FontColor(Colors.Grey.Darken2);
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Page ").FontSize(9);
                    t.CurrentPageNumber().FontSize(9);
                    t.Span(" of ").FontSize(9);
                    t.TotalPages().FontSize(9);
                });
            });
        });

        return document.GeneratePdf();
    }

    private async Task CreateNotificationsAsync(Policy policy, Customer customer, string templateKey, CancellationToken cancellationToken)
    {
        var templates = await dbContext.NotificationTemplates
            .Where(x => x.TemplateKey == templateKey && x.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var template in templates)
        {
            var subject = RenderTemplate(template.SubjectTemplate, policy);
            var body = RenderTemplate(template.BodyTemplate, policy);
            var recipient = template.Channel switch
            {
                "Email" => customer.Email,
                "SMS" => customer.PhoneNumber,
                "WhatsApp" => customer.PhoneNumber,
                _ => customer.Email
            };

            dbContext.PolicyNotifications.Add(new PolicyNotification
            {
                PolicyId = policy.Id,
                TemplateKey = templateKey,
                Channel = template.Channel,
                Recipient = string.IsNullOrWhiteSpace(recipient) ? "N/A" : recipient,
                Subject = subject,
                Body = body,
                Status = "Sent",
                SentAtUtc = DateTime.UtcNow
            });
        }
    }

    private static string RenderTemplate(string template, Policy policy)
    {
        return template
            .Replace("{PolicyNumber}", policy.PolicyNumber, StringComparison.Ordinal)
            .Replace("{PremiumAmount}", policy.PremiumAmount.ToString("F2"), StringComparison.Ordinal)
            .Replace("{ExpiryDate}", policy.ExpiryDate.ToString("yyyy-MM-dd"), StringComparison.Ordinal);
    }
}
