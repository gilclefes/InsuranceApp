using InsuranceApp.Domain.Entities;

namespace InsuranceApp.Web.Models;

public sealed class ProductConfigViewModel
{
    public ProductDefinition Product { get; set; } = new();
    public IReadOnlyCollection<ProductRiskRule> RiskRules { get; set; } = Array.Empty<ProductRiskRule>();
    public IReadOnlyCollection<ProductRider> Riders { get; set; } = Array.Empty<ProductRider>();
}
