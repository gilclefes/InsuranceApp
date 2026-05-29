using InsuranceApp.Application.Interfaces;
using InsuranceApp.Contracts.Agents;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InsuranceApp.Infrastructure.Agents;

public class AgentPortalService(InsuranceDbContext dbContext, IOptions<AgentCommissionOptions> options) : IAgentPortalService
{
    private readonly AgentCommissionOptions _options = options.Value;

    public async Task<AgentPortfolioResponse> GetPortfolioAsync(string agentUserId, CancellationToken cancellationToken = default)
    {
        var customers = await dbContext.Customers
            .Where(c => c.RegisteredByAgentId == agentUserId)
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var customerIds = customers.Select(c => c.Id).ToList();
        var policies = await dbContext.Policies
            .Where(p => customerIds.Contains(p.CustomerId) || p.AssignedAgentId == agentUserId)
            .ToListAsync(cancellationToken);

        var summaries = customers.Select(c => new AgentCustomerSummary
        {
            CustomerId = c.Id,
            CustomerNumber = c.CustomerNumber,
            FullName = $"{c.FirstName} {c.LastName}".Trim(),
            PhoneNumber = c.PhoneNumber,
            Email = c.Email,
            PolicyCount = policies.Count(p => p.CustomerId == c.Id),
            CreatedAtUtc = c.CreatedAtUtc
        }).ToList();

        return new AgentPortfolioResponse
        {
            AgentUserId = agentUserId,
            CustomersOnboarded = customers.Count,
            ActivePolicies = policies.Count(p => p.Status == PolicyStatus.Active),
            LapsedPolicies = policies.Count(p => p.Status == PolicyStatus.Lapsed),
            AnnualisedPremium = policies.Where(p => p.Status == PolicyStatus.Active).Sum(p => p.PremiumAmount),
            Customers = summaries
        };
    }

    public async Task<AgentCommissionLedgerResponse> GetCommissionLedgerAsync(string agentUserId, CancellationToken cancellationToken = default)
    {
        var rate = _options.DefaultCommissionRate;
        var policies = await dbContext.Policies
            .Include(p => p.Customer)
            .Where(p => p.AssignedAgentId == agentUserId && p.Status == PolicyStatus.Active)
            .OrderByDescending(p => p.InceptionDate)
            .ToListAsync(cancellationToken);

        var entries = policies.Select(p => new AgentCommissionEntry
        {
            PolicyNumber = p.PolicyNumber,
            ProductCode = p.ProductType.ToString(),
            CustomerName = p.Customer is null ? string.Empty : $"{p.Customer.FirstName} {p.Customer.LastName}".Trim(),
            PremiumAmount = p.PremiumAmount,
            CommissionAmount = Math.Round(p.PremiumAmount * rate, 2),
            Status = p.Status.ToString(),
            InceptionDate = p.InceptionDate
        }).ToList();

        return new AgentCommissionLedgerResponse
        {
            AgentUserId = agentUserId,
            CommissionRate = rate,
            TotalCommission = entries.Sum(e => e.CommissionAmount),
            Entries = entries
        };
    }
}
