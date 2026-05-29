namespace InsuranceApp.Contracts.Claims;

public sealed class ClaimTimelineEventResponse
{
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ActorUserId { get; set; } = string.Empty;
    public DateTime EventAtUtc { get; set; }
}