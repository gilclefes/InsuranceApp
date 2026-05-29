namespace InsuranceApp.Contracts.Auth;

public sealed class OtpRequestResponse
{
    public string ChallengeId { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public int RetryAfterSeconds { get; set; }
}
