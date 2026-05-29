namespace InsuranceApp.Contracts.Products;

public sealed class CreateProductRiskRuleRequest
{
    public string ParameterName { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public decimal ThresholdValue { get; set; }
    public string AdjustmentType { get; set; } = "Percent";
    public decimal AdjustmentValue { get; set; }
    public string Reason { get; set; } = string.Empty;
}
