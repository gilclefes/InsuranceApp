using InsuranceApp.Domain.Common;

namespace InsuranceApp.Domain.Entities;

public class ClaimTimelineEvent : BaseAuditableEntity
{
    public long Id { get; set; }
    public long ClaimId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ActorUserId { get; set; } = string.Empty;
    public DateTime EventAtUtc { get; set; }

    public Claim? Claim { get; set; }
}