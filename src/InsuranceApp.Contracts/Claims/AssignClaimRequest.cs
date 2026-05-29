namespace InsuranceApp.Contracts.Claims;

public sealed class AssignClaimRequest
{
    public string AdjusterUserId { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}