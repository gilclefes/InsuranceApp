namespace InsuranceApp.Contracts.Quotes;

public sealed class GenerateQuoteRequest
{
    public string ProductCode { get; set; } = string.Empty;
    public decimal CoverageAmount { get; set; }
    public int ApplicantAge { get; set; }
    public decimal VehicleValue { get; set; }
    public decimal SumAssured { get; set; }
    public IReadOnlyCollection<string> SelectedRiderCodes { get; set; } = [];
}
