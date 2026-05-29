using InsuranceApp.Domain.Common;

namespace InsuranceApp.Domain.Entities;

public class PolicyDocument : BaseAuditableEntity
{
    public long Id { get; set; }
    public long PolicyId { get; set; }
    public string DocumentReference { get; set; } = string.Empty;
    public string DocumentType { get; set; } = "PolicySchedule";
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
    public string Status { get; set; } = "Generated";
    public byte[] Content { get; set; } = [];
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;

    public Policy? Policy { get; set; }
}
