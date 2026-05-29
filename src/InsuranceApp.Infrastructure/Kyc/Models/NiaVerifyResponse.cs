namespace InsuranceApp.Infrastructure.Kyc.Models;

public sealed class NiaVerifyResponse
{
    public string? Status { get; set; }
    public string? VerificationStatus { get; set; }
    public bool? IsVerified { get; set; }
    public string? Message { get; set; }
    public string? ResponseCode { get; set; }
}
