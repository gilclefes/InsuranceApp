namespace InsuranceApp.Contracts.Onboarding;

public sealed class CustomerProfileResponse
{
    public long CustomerId { get; set; }
    public string CustomerNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool KycVerified { get; set; }
    public bool ConsentAccepted { get; set; }
    public DateTime? ConsentAcceptedAtUtc { get; set; }
    public bool RegisteredByAgent { get; set; }
    public string RegisteredByAgentId { get; set; } = string.Empty;
}
