namespace InsuranceApp.Contracts.Quotes;

public sealed class GenerateQuoteResponse
{
    public string QuoteReference { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public decimal BasePremium { get; set; }
    public decimal TotalPremium { get; set; }
    public string CurrencyCode { get; set; } = "GHS";
    public DateTime ValidUntilUtc { get; set; }
    public IReadOnlyCollection<QuoteAdjustment> Adjustments { get; set; } = [];
    public IReadOnlyCollection<QuoteRiderAdjustment> AppliedRiders { get; set; } = [];
}

public sealed class QuoteAdjustment
{
    public string Reason { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public sealed class QuoteRiderAdjustment
{
    public string RiderCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
