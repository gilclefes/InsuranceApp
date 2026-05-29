using InsuranceApp.Contracts.PremiumCollections;
using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Web.Models;

public sealed class PremiumAdminViewModel
{
    [Required]
    public string PolicyNumber { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateTime DueDateUtc { get; set; } = DateTime.UtcNow.Date;

    [Range(0.01, 1_000_000)]
    public decimal? Amount { get; set; }

    public string Provider { get; set; } = "MTN_MOMO";
    public string PaymentChannel { get; set; } = "MoMo";

    public IReadOnlyCollection<PremiumCollectionItemResponse> Items { get; set; } = Array.Empty<PremiumCollectionItemResponse>();
}