namespace InsuranceApp.Application.Common;

public sealed class AuthOptions
{
    public bool RequireOtpOnLogin { get; set; } = true;
    public bool OtpForHighRiskDevicesOnly { get; set; } = true;
    public int TrustedDeviceWindowDays { get; set; } = 90;
}
