using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Contracts.Products;

public sealed class UpdateProductDefinitionRequest
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(1000)]
    public string CoverageSummary { get; set; } = string.Empty;

    [StringLength(20)]
    public string NicClassCode { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(8)]
    public string CurrencyCode { get; set; } = "GHS";

    [Range(0, 1)]
    public decimal BaseRate { get; set; }

    [Range(0, 100000000)]
    public decimal MinPremium { get; set; }

    [Range(0, 1000000000)]
    public decimal MaxCoverageAmount { get; set; }

    [Range(0, 120)]
    public int MinEntryAgeYears { get; set; }

    [Range(0, 120)]
    public int MaxEntryAgeYears { get; set; } = 70;

    [StringLength(100)]
    public string PolicyTermOptions { get; set; } = "Annual";

    [Range(0, 3650)]
    public int WaitingPeriodDays { get; set; }

    [StringLength(4000)]
    public string Exclusions { get; set; } = string.Empty;

    [StringLength(4000)]
    public string UnderwritingRequirements { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
