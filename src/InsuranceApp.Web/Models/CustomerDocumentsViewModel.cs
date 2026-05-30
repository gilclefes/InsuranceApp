namespace InsuranceApp.Web.Models;

public sealed class CustomerDocumentsViewModel
{
    public string CustomerNumberFilter { get; set; } = string.Empty;
    public string DocumentTypeFilter { get; set; } = string.Empty;
    public IReadOnlyCollection<CustomerDocumentListItem> Documents { get; set; } = Array.Empty<CustomerDocumentListItem>();
}

public sealed class CustomerDocumentListItem
{
    public string DocumentReference { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string CustomerNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string StorageProvider { get; set; } = string.Empty;
    public string StorageUrl { get; set; } = string.Empty;
    public string UploadedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string ClaimNumber { get; set; } = string.Empty;
}
