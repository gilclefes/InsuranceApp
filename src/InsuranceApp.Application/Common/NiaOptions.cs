namespace InsuranceApp.Application.Common;

public sealed class NiaOptions
{
    public bool UseMock { get; set; } = true;
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public bool EnforceSignatureValidation { get; set; }
    public string SignatureHeaderName { get; set; } = "X-Signature";
    public string SignatureSecret { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 10;
    public int RetryCount { get; set; } = 3;
    public int CircuitBreakerFailureThreshold { get; set; } = 5;
    public int CircuitBreakerDurationSeconds { get; set; } = 60;
}
