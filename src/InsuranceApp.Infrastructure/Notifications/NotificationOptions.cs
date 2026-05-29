namespace InsuranceApp.Infrastructure.Notifications;

public sealed class NotificationOptions
{
    public string PreferredSmsChannel { get; set; } = "LOGGING";
    public string PreferredEmailChannel { get; set; } = "LOGGING";
    public string PreferredWhatsAppChannel { get; set; } = "LOGGING";
    public HubtelSmsOptions Hubtel { get; set; } = new();
    public SmtpOptions Smtp { get; set; } = new();
    public WhatsAppOptions WhatsApp { get; set; } = new();
}

public sealed class HubtelSmsOptions
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = "https://sms.hubtel.com";
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string From { get; set; } = "InsuranceApp";
}

public sealed class SmtpOptions
{
    public bool Enabled { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "no-reply@insuranceapp.gh";
    public string FromName { get; set; } = "InsuranceApp";
}

public sealed class WhatsAppOptions
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = "https://graph.facebook.com/v20.0";
    public string PhoneNumberId { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
}
