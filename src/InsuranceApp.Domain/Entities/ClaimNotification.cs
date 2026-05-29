using InsuranceApp.Domain.Common;

namespace InsuranceApp.Domain.Entities;

public class ClaimNotification : BaseAuditableEntity
{
    public long Id { get; set; }
    public long ClaimId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SentAtUtc { get; set; }

    public Claim? Claim { get; set; }
}