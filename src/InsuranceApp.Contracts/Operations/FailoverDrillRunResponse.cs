namespace InsuranceApp.Contracts.Operations;

public sealed class FailoverDrillRunResponse
{
    public DateTime StartedAtUtc { get; set; }
    public DateTime CompletedAtUtc { get; set; }
    public bool Successful { get; set; }
    public IReadOnlyCollection<FailoverDrillStepResponse> Steps { get; set; } = Array.Empty<FailoverDrillStepResponse>();
}