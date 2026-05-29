namespace InsuranceApp.Contracts.Policies;

public sealed class PolicyDocumentResponse
{
    public string PolicyNumber { get; set; } = string.Empty;
    public string DocumentReference { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
    public byte[] Content { get; set; } = [];
}
