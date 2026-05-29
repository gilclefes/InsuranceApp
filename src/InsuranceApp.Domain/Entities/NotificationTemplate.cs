using InsuranceApp.Domain.Common;

namespace InsuranceApp.Domain.Entities;

public class NotificationTemplate : BaseAuditableEntity
{
    public long Id { get; set; }
    public string TemplateKey { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string SubjectTemplate { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
