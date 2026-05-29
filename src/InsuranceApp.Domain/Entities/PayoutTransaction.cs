using InsuranceApp.Domain.Common;
using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Domain.Entities;

public class PayoutTransaction : BaseAuditableEntity
{
    public long Id { get; set; }
    public long ClaimId { get; set; }
    public string PayoutReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Initiated;
    public string DestinationChannel { get; set; } = string.Empty;
    public DateTime? DisbursedAtUtc { get; set; }

    public Claim? Claim { get; set; }
}
