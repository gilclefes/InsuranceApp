using InsuranceApp.Contracts.PremiumCollections;
using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Web.Models;

public sealed class PremiumAdminViewModel
{
    [Required(AllowEmptyStrings = false)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Policy number cannot be empty.")]
    [StringLength(40)]
    public string PolicyNumber { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateTime DueDateUtc { get; set; } = DateTime.UtcNow.Date;

    [Range(0.01, 1_000_000)]
    public decimal? Amount { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(40)]
    public string Provider { get; set; } = "MTN_MOMO";

    [Required(AllowEmptyStrings = false)]
    [StringLength(32)]
    public string PaymentChannel { get; set; } = "MoMo";
    [StringLength(40)]
    public string WebhookEventId { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false)]
    [StringLength(80)]
    public string WebhookTransactionReference { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false)]
    [StringLength(20)]
    public string WebhookStatus { get; set; } = "success";

    [DataType(DataType.Date)]
    public DateTime ReconciliationFromUtc { get; set; } = DateTime.UtcNow.Date.AddDays(-7);

    [DataType(DataType.Date)]
    public DateTime ReconciliationToUtc { get; set; } = DateTime.UtcNow.Date;

    public IReadOnlyCollection<PremiumCollectionItemResponse> Items { get; set; } = Array.Empty<PremiumCollectionItemResponse>();
    public PremiumReconciliationSummaryResponse? Reconciliation { get; set; }
}