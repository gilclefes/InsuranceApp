using InsuranceApp.Contracts.Agents;
using InsuranceApp.Contracts.Onboarding;
using Microsoft.AspNetCore.Http;

namespace InsuranceApp.Web.Models;

public class AgentPortfolioViewModel
{
    public AgentPortfolioResponse Portfolio { get; set; } = new();
}

public class AgentCommissionViewModel
{
    public AgentCommissionLedgerResponse Ledger { get; set; } = new();
}

public class AgentOnboardViewModel
{
    public AgentOnboardCustomerRequest Request { get; set; } = new()
    {
        DateOfBirth = DateTime.UtcNow.Date.AddYears(-18)
    };
    public IFormFile? CustomerIntakeForm { get; set; }
}
