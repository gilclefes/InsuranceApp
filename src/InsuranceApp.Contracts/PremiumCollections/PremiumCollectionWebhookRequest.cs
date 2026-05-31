using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Contracts.PremiumCollections;

public sealed class PremiumCollectionWebhookRequest
{
    [StringLength(40)]
    public string Provider { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(40)]
    public string EventId { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(80)]
    public string TransactionReference { get; set; } = string.Empty;

    [StringLength(40)]
    public string PolicyNumber { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(20)]
    public string Status { get; set; } = string.Empty;

    [Range(0.01, 100000000)]
    public decimal? Amount { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime EventTimeUtc { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(256)]
    public string Signature { get; set; } = string.Empty;

    [StringLength(10000)]
    public string RawPayload { get; set; } = string.Empty;
}