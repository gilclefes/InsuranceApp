using InsuranceApp.Domain.Common;
using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Domain.Entities;

public class Claim : BaseAuditableEntity
{
    public long Id { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public long PolicyId { get; set; }
    public DateTime IncidentDate { get; set; }
    public string ClaimType { get; set; } = string.Empty;
    public ClaimStatus Status { get; set; } = ClaimStatus.Filed;
    public decimal ClaimedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }

    public Policy? Policy { get; set; }
    public ICollection<PayoutTransaction> PayoutTransactions { get; set; } = new List<PayoutTransaction>();
}
