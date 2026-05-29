using InsuranceApp.Domain.Common;

namespace InsuranceApp.Domain.Entities;

public class PayoutWebhookLog : BaseAuditableEntity
{
    public long Id { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string PayoutReference { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string ProcessingStatus { get; set; } = string.Empty;
    public DateTime ReceivedAtUtc { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
}