namespace InsuranceApp.Contracts.Products;

public sealed class CreateProductDefinitionRequest
{
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int ProductType { get; set; }
    public string CurrencyCode { get; set; } = "GHS";
    public decimal BaseRate { get; set; }
    public decimal MinPremium { get; set; }
}
