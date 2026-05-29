namespace InsuranceApp.Contracts.Operations;

public sealed class FailoverDrillStepResponse
{
    public string StepName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public long DurationMs { get; set; }
}