namespace InsuranceApp.Contracts.Auth;

public sealed class AuthRequestContext
{
    public string DeviceId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}
