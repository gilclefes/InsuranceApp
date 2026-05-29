using InsuranceApp.Contracts.Agents;
using InsuranceApp.Contracts.Onboarding;

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
    public AgentOnboardCustomerRequest Request { get; set; } = new();
}
