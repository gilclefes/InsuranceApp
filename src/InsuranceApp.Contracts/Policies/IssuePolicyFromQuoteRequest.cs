using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Contracts.Policies;

public sealed class IssuePolicyFromQuoteRequest
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(40)]
    public string QuoteReference { get; set; } = string.Empty;

    [Required]
    [Range(1, long.MaxValue)]
    public long CustomerId { get; set; }

    [StringLength(64)]
    public string AssignedAgentId { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string CoverageType { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateTime InceptionDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime ExpiryDate { get; set; }
}
