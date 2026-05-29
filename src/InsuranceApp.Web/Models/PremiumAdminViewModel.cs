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
    public string WebhookEventId { get; set; } = string.Empty;
    public string WebhookTransactionReference { get; set; } = string.Empty;
    public string WebhookStatus { get; set; } = "success";
    public DateTime ReconciliationFromUtc { get; set; } = DateTime.UtcNow.Date.AddDays(-7);
    public DateTime ReconciliationToUtc { get; set; } = DateTime.UtcNow.Date;

    public IReadOnlyCollection<PremiumCollectionItemResponse> Items { get; set; } = Array.Empty<PremiumCollectionItemResponse>();
    public PremiumReconciliationSummaryResponse? Reconciliation { get; set; }
}