namespace InsuranceApp.Contracts.Products;

public sealed class ProductDefinitionResponse
{
    public long Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int ProductType { get; set; }
    public string CurrencyCode { get; set; } = "GHS";
    public decimal BaseRate { get; set; }
    public decimal MinPremium { get; set; }
    public bool IsActive { get; set; }
}
