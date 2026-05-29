namespace InsuranceApp.Contracts.Claims;

public sealed class CreateClaimRequest
{
    public string PolicyNumber { get; set; } = string.Empty;
    public DateTime IncidentDate { get; set; }
    public string ClaimType { get; set; } = string.Empty;
    public decimal ClaimedAmount { get; set; }
    public string EvidenceUrl { get; set; } = string.Empty;
}