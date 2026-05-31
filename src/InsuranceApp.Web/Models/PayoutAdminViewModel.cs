using InsuranceApp.Contracts.Payouts;
using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Web.Models;

public sealed class PayoutAdminViewModel
{
    public string? StatusFilter { get; set; }
    public string? ClaimFilter { get; set; }

    [Required(AllowEmptyStrings = false)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Claim number cannot be empty.")]
    [StringLength(40)]
    public string ClaimNumber { get; set; } = string.Empty;

    [Range(0.01, 100000000)]
    public decimal? Amount { get; set; }

    [Required(AllowEmptyStrings = false)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Destination channel cannot be empty.")]
    [StringLength(32)]
    public string DestinationChannel { get; set; } = "MoMo";

    [Required(AllowEmptyStrings = false)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Destination account cannot be empty.")]
    [StringLength(120)]
    public string DestinationAccount { get; set; } = string.Empty;

    [StringLength(80)]
    public string ExternalReference { get; set; } = string.Empty;

    [StringLength(40)]
    public string WebhookEventId { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Webhook payout reference cannot be empty.")]
    [StringLength(60)]
    public string WebhookPayoutReference { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false)]
    [StringLength(20)]
    public string WebhookStatus { get; set; } = "success";

    public IReadOnlyCollection<PayoutResponse> Payouts { get; set; } = Array.Empty<PayoutResponse>();
}