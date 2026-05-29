namespace InsuranceApp.Contracts.Auth;

public sealed class OtpVerifyResponse
{
    public bool IsVerified { get; set; }
    public bool IsLocked { get; set; }
    public int RemainingAttempts { get; set; }
    public string Message { get; set; } = string.Empty;
}
