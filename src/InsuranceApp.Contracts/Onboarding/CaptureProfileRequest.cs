namespace InsuranceApp.Contracts.Onboarding;

public sealed class CaptureProfileRequest
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string GhanaCardNumber { get; set; } = string.Empty;
    public bool ConsentAccepted { get; set; }
}
