using InsuranceApp.Contracts.Payouts;
using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Web.Models;

public sealed class PayoutAdminViewModel
{
    public string? StatusFilter { get; set; }
    public string? ClaimFilter { get; set; }

    [Required]
    public string ClaimNumber { get; set; } = string.Empty;

    [Range(0.01, 100000000)]
    public decimal? Amount { get; set; }

    public string DestinationChannel { get; set; } = "MoMo";

    [Required]
    public string DestinationAccount { get; set; } = string.Empty;

    public string ExternalReference { get; set; } = string.Empty;

    public string WebhookEventId { get; set; } = string.Empty;
    public string WebhookPayoutReference { get; set; } = string.Empty;
    public string WebhookStatus { get; set; } = "success";

    public IReadOnlyCollection<PayoutResponse> Payouts { get; set; } = Array.Empty<PayoutResponse>();
}