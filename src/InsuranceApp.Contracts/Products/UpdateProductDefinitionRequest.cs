namespace InsuranceApp.Contracts.Products;

public sealed class UpdateProductDefinitionRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CoverageSummary { get; set; } = string.Empty;
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
}
