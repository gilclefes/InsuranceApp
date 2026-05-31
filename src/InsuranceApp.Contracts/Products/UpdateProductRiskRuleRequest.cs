using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Contracts.Products;

public sealed class UpdateProductRiskRuleRequest
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string ParameterName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(4)]
    public string Operator { get; set; } = string.Empty;
    public decimal ThresholdValue { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(20)]
    public string AdjustmentType { get; set; } = "Percent";

    [Range(-100000000, 100000000)]
    public decimal AdjustmentValue { get; set; }

    [StringLength(250)]
    public string Reason { get; set; } = string.Empty;
}
