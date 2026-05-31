using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Contracts.Products;

public sealed class CreateProductRiderRequest
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(32)]
    public string RiderCode { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(20)]
    public string AdjustmentType { get; set; } = "Flat";

    [Range(-100000000, 100000000)]
    public decimal AdjustmentValue { get; set; }
}
