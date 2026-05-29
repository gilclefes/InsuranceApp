namespace InsuranceApp.Contracts.Policies;

public sealed class PolicyOperationResponse
{
    public string PolicyNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
