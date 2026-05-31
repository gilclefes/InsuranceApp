using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Contracts.PremiumCollections;

public sealed class CreatePremiumMandateRequest
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(40)]
    public string PolicyNumber { get; set; } = string.Empty;

    [StringLength(40)]
    public string Provider { get; set; } = string.Empty;

    [StringLength(32)]
    public string PaymentChannel { get; set; } = "MoMo";

    [StringLength(80)]
    public string ExternalReference { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime? FirstDueDateUtc { get; set; }
}