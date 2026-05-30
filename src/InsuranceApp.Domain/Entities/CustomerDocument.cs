using InsuranceApp.Domain.Common;

namespace InsuranceApp.Domain.Entities;

public class CustomerDocument : BaseAuditableEntity
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public long? PolicyId { get; set; }
    public long? ClaimId { get; set; }

    public string DocumentReference { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string StorageProvider { get; set; } = string.Empty;
    public string StorageUrl { get; set; } = string.Empty;
    public string UploadedByUserId { get; set; } = string.Empty;

    public Customer? Customer { get; set; }
    public Policy? Policy { get; set; }
    public Claim? Claim { get; set; }
}
