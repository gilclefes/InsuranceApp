namespace InsuranceApp.Contracts.Claims;

public sealed class ClaimResponse
{
    public string ClaimNumber { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public DateTime IncidentDate { get; set; }
    public string ClaimType { get; set; } = string.Empty;
    public decimal ClaimedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public string EvidenceUrl { get; set; } = string.Empty;
    public string AssignedAdjusterId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string DecisionReason { get; set; } = string.Empty;
    public string ReviewNotes { get; set; } = string.Empty;
    public bool IsFraudRisk { get; set; }
    public decimal FraudScore { get; set; }
    public string FraudReason { get; set; } = string.Empty;
    public bool IsSlaBreached { get; set; }
    public int AgeHours { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? DecisionedAtUtc { get; set; }
}