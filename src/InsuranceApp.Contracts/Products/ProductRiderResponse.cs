namespace InsuranceApp.Contracts.Products;

public sealed class ProductRiderResponse
{
    public long Id { get; set; }
    public long ProductDefinitionId { get; set; }
    public string RiderCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AdjustmentType { get; set; } = string.Empty;
    public decimal AdjustmentValue { get; set; }
    public bool IsActive { get; set; }
}
