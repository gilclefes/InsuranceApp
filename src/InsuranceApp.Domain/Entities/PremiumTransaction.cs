using InsuranceApp.Domain.Common;
using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Domain.Entities;

public class PremiumTransaction : BaseAuditableEntity
{
    public long Id { get; set; }
    public long PolicyId { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string PaymentChannel { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string IdempotencyKey { get; set; } = string.Empty;
    public string ProviderReference { get; set; } = string.Empty;
    public string FailureReason { get; set; } = string.Empty;
    public DateTime DueDateUtc { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
    public int RetryCount { get; set; }

    public Policy? Policy { get; set; }
}
