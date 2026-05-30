using InsuranceApp.Domain.Common;
using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Domain.Entities;

public class ProductDefinition : BaseAuditableEntity
{
    public long Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CoverageSummary { get; set; } = string.Empty;
    public ProductType ProductType { get; set; }
    public string NicClassCode { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "GHS";
    public decimal BaseRate { get; set; }
    public decimal MinPremium { get; set; }
    public decimal MaxCoverageAmount { get; set; }
    public int MinEntryAgeYears { get; set; }
    public int MaxEntryAgeYears { get; set; } = 70;
    public string PolicyTermOptions { get; set; } = "Annual";
    public int WaitingPeriodDays { get; set; }
    public string Exclusions { get; set; } = string.Empty;
    public string UnderwritingRequirements { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<ProductRiskRule> RiskRules { get; set; } = new List<ProductRiskRule>();
    public ICollection<ProductRider> Riders { get; set; } = new List<ProductRider>();
}
