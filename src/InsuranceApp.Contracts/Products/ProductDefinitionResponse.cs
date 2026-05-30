namespace InsuranceApp.Contracts.Products;

public sealed class ProductDefinitionResponse
{
    public long Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CoverageSummary { get; set; } = string.Empty;
    public int ProductType { get; set; }
    public string NicClassCode { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "GHS";
    public decimal BaseRate { get; set; }
    public decimal MinPremium { get; set; }
    public decimal MaxCoverageAmount { get; set; }
    public int MinEntryAgeYears { get; set; }
    public int MaxEntryAgeYears { get; set; }
    public string PolicyTermOptions { get; set; } = string.Empty;
    public int WaitingPeriodDays { get; set; }
    public string Exclusions { get; set; } = string.Empty;
    public string UnderwritingRequirements { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
