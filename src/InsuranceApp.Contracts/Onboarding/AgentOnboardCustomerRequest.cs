using InsuranceApp.Contracts.Auth;

namespace InsuranceApp.Contracts.Onboarding;

public sealed class AgentOnboardCustomerRequest
{
    public RegisterRequest Register { get; set; } = new();
    public DateTime DateOfBirth { get; set; } = DateTime.UtcNow.Date.AddYears(-18);
    public string GhanaCardNumber { get; set; } = string.Empty;
    public bool ConsentAccepted { get; set; }
}
