namespace InsuranceApp.Contracts.Ussd;

public sealed class UssdRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string ServiceCode { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public sealed class UssdResponse
{
    public string Message { get; set; } = string.Empty;
    public bool EndSession { get; set; }
}
