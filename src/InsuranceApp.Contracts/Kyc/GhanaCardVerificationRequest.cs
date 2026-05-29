namespace InsuranceApp.Contracts.Kyc;

public sealed class GhanaCardVerificationRequest
{
    public string GhanaCardNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}
