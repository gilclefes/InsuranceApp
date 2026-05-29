namespace InsuranceApp.Contracts.Auth;

public sealed class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string OtpChallengeId { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
}
