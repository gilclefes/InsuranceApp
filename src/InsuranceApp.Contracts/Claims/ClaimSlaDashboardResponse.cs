namespace InsuranceApp.Contracts.Claims;

public sealed class ClaimSlaDashboardResponse
{
    public int TotalOpenClaims { get; set; }
    public int BreachedClaims { get; set; }
    public int HighFraudRiskClaims { get; set; }
    public decimal BreachPercentage { get; set; }
    public IReadOnlyCollection<ClaimResponse> OpenQueue { get; set; } = Array.Empty<ClaimResponse>();
}