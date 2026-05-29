namespace InsuranceApp.Contracts.Quotes;

public sealed class QuoteRecordResponse
{
    public string QuoteReference { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public decimal CoverageAmount { get; set; }
    public int ApplicantAge { get; set; }
    public decimal VehicleValue { get; set; }
    public decimal SumAssured { get; set; }
    public decimal BasePremium { get; set; }
    public decimal TotalPremium { get; set; }
    public string CurrencyCode { get; set; } = "GHS";
    public DateTime ValidUntilUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public string IssuedPolicyNumber { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
