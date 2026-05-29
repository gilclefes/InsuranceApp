using InsuranceApp.Domain.Common;

namespace InsuranceApp.Domain.Entities;

public class IdentityAuditLog : BaseAuditableEntity
{
    public long Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public string SubjectId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}
