namespace InsuranceApp.Contracts.Products;

public sealed class UpdateProductRiderRequest
{
    public string Name { get; set; } = string.Empty;
    public string AdjustmentType { get; set; } = "Flat";
    public decimal AdjustmentValue { get; set; }
    public bool IsActive { get; set; } = true;
}
