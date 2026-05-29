namespace InsuranceApp.Application.Common;

public sealed class OtpOptions
{
    public int CodeLength { get; set; } = 6;
    public int ExpiryMinutes { get; set; } = 5;
    public int MaxAttempts { get; set; } = 5;
    public int ThrottleSeconds { get; set; } = 60;
    public int LockoutMinutes { get; set; } = 15;
    public int MaxRequestsPerHour { get; set; } = 5;
}
