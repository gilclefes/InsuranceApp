namespace InsuranceApp.Contracts.Auth;

public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
}
