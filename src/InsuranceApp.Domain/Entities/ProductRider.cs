using InsuranceApp.Domain.Common;

namespace InsuranceApp.Domain.Entities;

public class ProductRider : BaseAuditableEntity
{
    public long Id { get; set; }
    public long ProductDefinitionId { get; set; }
    public string RiderCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AdjustmentType { get; set; } = "Flat";
    public decimal AdjustmentValue { get; set; }
    public bool IsActive { get; set; } = true;

    public ProductDefinition? ProductDefinition { get; set; }
}
