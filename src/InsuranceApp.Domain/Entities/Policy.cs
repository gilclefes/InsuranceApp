using InsuranceApp.Domain.Common;
using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Domain.Entities;

public class Policy : BaseAuditableEntity
{
    public long Id { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public long CustomerId { get; set; }
    public ProductType ProductType { get; set; }
    public string AssignedAgentId { get; set; } = string.Empty;
    public string CoverageType { get; set; } = string.Empty;
    public decimal PremiumAmount { get; set; }
    public string CurrencyCode { get; set; } = "GHS";
    public DateTime InceptionDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string CancellationReason { get; set; } = string.Empty;
    public PolicyStatus Status { get; set; } = PolicyStatus.Draft;

    public Customer? Customer { get; set; }
    public ICollection<Claim> Claims { get; set; } = new List<Claim>();
    public ICollection<PremiumTransaction> PremiumTransactions { get; set; } = new List<PremiumTransaction>();
}
