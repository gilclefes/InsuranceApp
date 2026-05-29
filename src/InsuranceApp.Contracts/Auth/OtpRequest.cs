namespace InsuranceApp.Contracts.Auth;

public sealed class OtpRequest
{
    public string Destination { get; set; } = string.Empty;
    public string Purpose { get; set; } = "Login";
    public string Channel { get; set; } = "SMS";
}
