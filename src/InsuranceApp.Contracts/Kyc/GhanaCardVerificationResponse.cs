namespace InsuranceApp.Contracts.Kyc;

public sealed class GhanaCardVerificationResponse
{
    public bool IsVerified { get; set; }
    public string VerificationStatus { get; set; } = "Pending";
    public string Message { get; set; } = string.Empty;
}
