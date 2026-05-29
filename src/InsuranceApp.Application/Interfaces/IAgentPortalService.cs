using InsuranceApp.Contracts.Agents;

namespace InsuranceApp.Application.Interfaces;

public interface IAgentPortalService
{
    Task<AgentPortfolioResponse> GetPortfolioAsync(string agentUserId, CancellationToken cancellationToken = default);
    Task<AgentCommissionLedgerResponse> GetCommissionLedgerAsync(string agentUserId, CancellationToken cancellationToken = default);
}
