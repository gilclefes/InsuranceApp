using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Contracts.Quotes;

public sealed class GenerateQuoteRequest
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(32)]
    public string ProductCode { get; set; } = string.Empty;

    [Range(0, 1000000000)]
    public decimal CoverageAmount { get; set; }

    [Range(0, 120)]
    public int ApplicantAge { get; set; }

    [Range(0, 1000000000)]
    public decimal VehicleValue { get; set; }

    [Range(0, 1000000000)]
    public decimal SumAssured { get; set; }
    public IReadOnlyCollection<string> SelectedRiderCodes { get; set; } = [];
}
