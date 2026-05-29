using InsuranceApp.Domain.Common;

namespace InsuranceApp.Domain.Entities;

public class QuoteRecord : BaseAuditableEntity
{
    public long Id { get; set; }
    public string QuoteReference { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public decimal CoverageAmount { get; set; }
    public int ApplicantAge { get; set; }
    public decimal VehicleValue { get; set; }
    public decimal SumAssured { get; set; }
    public decimal BasePremium { get; set; }
    public decimal TotalPremium { get; set; }
    public string CurrencyCode { get; set; } = "GHS";
    public string SelectedRiderCodes { get; set; } = string.Empty;
    public DateTime ValidUntilUtc { get; set; }
    public string Status { get; set; } = "Quoted";
    public string IssuedPolicyNumber { get; set; } = string.Empty;
}
