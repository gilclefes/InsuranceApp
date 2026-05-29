namespace InsuranceApp.Contracts.Compliance;

public sealed class ComplianceExceptionItemResponse
{
    public string Category { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
}