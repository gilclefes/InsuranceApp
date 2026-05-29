using InsuranceApp.Domain.Common;

namespace InsuranceApp.Domain.Entities;

public class PolicyNotification : BaseAuditableEntity
{
    public long Id { get; set; }
    public long PolicyId { get; set; }
    public string TemplateKey { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Status { get; set; } = "Sent";
    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;

    public Policy? Policy { get; set; }
}
