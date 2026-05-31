using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.Contracts.Audit;

public sealed class IdentityAuditLogQueryRequest
{
    [StringLength(100)]
    public string Action { get; set; } = string.Empty;

    [StringLength(50)]
    public string Outcome { get; set; } = string.Empty;

    [StringLength(100)]
    public string SubjectId { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime? FromUtc { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ToUtc { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 5000)]
    public int PageSize { get; set; } = 50;
}
