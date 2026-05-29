namespace InsuranceApp.Infrastructure.Kyc.Models;

public sealed class NiaVerifyRequest
{
    public string GhanaCardNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
}
