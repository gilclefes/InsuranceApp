using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Contracts.PremiumCollections;

public sealed class SchedulePremiumCollectionRequest
{
    [Required]
    [DataType(DataType.Date)]
    public DateTime DueDateUtc { get; set; }

    [Range(0.01, 100000000)]
    public decimal? Amount { get; set; }

    [StringLength(40)]
    public string? Provider { get; set; }

    [StringLength(32)]
    public string? PaymentChannel { get; set; }
}