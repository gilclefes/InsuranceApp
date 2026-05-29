namespace InsuranceApp.Application.Common;

public sealed class OtpProviderOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string SenderId { get; set; } = "InsuranceApp";
    public string EndpointPath { get; set; } = "/messages/otp";
    public bool UseLoggingFallback { get; set; } = true;
}
