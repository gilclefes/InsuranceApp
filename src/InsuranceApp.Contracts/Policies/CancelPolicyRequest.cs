namespace InsuranceApp.Contracts.Policies;

public sealed class CancelPolicyRequest
{
    public string Reason { get; set; } = string.Empty;
}
