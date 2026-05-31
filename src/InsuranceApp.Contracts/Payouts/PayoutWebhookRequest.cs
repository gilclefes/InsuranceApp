using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Contracts.Payouts;

public sealed class PayoutWebhookRequest
{
    [StringLength(40)]
    public string Provider { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(40)]
    public string EventId { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(60)]
    public string PayoutReference { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(20)]
    public string Status { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(256)]
    public string Signature { get; set; } = string.Empty;

    [StringLength(10000)]
    public string RawPayload { get; set; } = string.Empty;

    [DataType(DataType.DateTime)]
    public DateTime EventTimeUtc { get; set; }
}