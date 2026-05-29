namespace InsuranceApp.Contracts.Audit;

public sealed class IdentityAuditLogQueryRequest
{
    public string Action { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public string SubjectId { get; set; } = string.Empty;
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
