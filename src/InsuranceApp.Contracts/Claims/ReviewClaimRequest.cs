namespace InsuranceApp.Contracts.Claims;

public sealed class ReviewClaimRequest
{
    public string Action { get; set; } = string.Empty;
    public decimal? ApprovedAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
}