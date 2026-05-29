namespace InsuranceApp.Contracts.Agents;

public sealed class AgentPortfolioResponse
{
    public string AgentUserId { get; set; } = string.Empty;
    public int CustomersOnboarded { get; set; }
    public int ActivePolicies { get; set; }
    public int LapsedPolicies { get; set; }
    public decimal AnnualisedPremium { get; set; }
    public IReadOnlyCollection<AgentCustomerSummary> Customers { get; set; } = Array.Empty<AgentCustomerSummary>();
}

public sealed class AgentCustomerSummary
{
    public long CustomerId { get; set; }
    public string CustomerNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int PolicyCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class AgentCommissionLedgerResponse
{
    public string AgentUserId { get; set; } = string.Empty;
    public decimal CommissionRate { get; set; }
    public decimal TotalCommission { get; set; }
    public IReadOnlyCollection<AgentCommissionEntry> Entries { get; set; } = Array.Empty<AgentCommissionEntry>();
}

public sealed class AgentCommissionEntry
{
    public string PolicyNumber { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal PremiumAmount { get; set; }
    public decimal CommissionAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime InceptionDate { get; set; }
}
