namespace InsuranceApp.Contracts.Products;

public sealed class UpdateProductDefinitionRequest
{
    public string Name { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "GHS";
    public decimal BaseRate { get; set; }
    public decimal MinPremium { get; set; }
    public bool IsActive { get; set; } = true;
}
