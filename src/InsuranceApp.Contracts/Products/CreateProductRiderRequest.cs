namespace InsuranceApp.Contracts.Products;

public sealed class CreateProductRiderRequest
{
    public string RiderCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AdjustmentType { get; set; } = "Flat";
    public decimal AdjustmentValue { get; set; }
}
