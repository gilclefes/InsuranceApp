namespace InsuranceApp.Infrastructure.Payments;

public sealed class PaymentGatewayOptions
{
    public string DefaultProvider { get; set; } = "SIMULATED";
    public HubtelOptions Hubtel { get; set; } = new();
    public MtnMomoOptions MtnMomo { get; set; } = new();
    public PaystackOptions Paystack { get; set; } = new();
    public FlutterwaveOptions Flutterwave { get; set; } = new();
    public GhIpssOptions GhIpss { get; set; } = new();
    public TelecelOptions Telecel { get; set; } = new();
    public AirtelTigoOptions AirtelTigo { get; set; } = new();
    public ZeepayOptions Zeepay { get; set; } = new();
}

public class HttpProviderOptions
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 15;
}

public sealed class HubtelOptions : HttpProviderOptions
{
    public string MerchantAccountNumber { get; set; } = string.Empty;
}

public sealed class MtnMomoOptions : HttpProviderOptions
{
    public string SubscriptionKey { get; set; } = string.Empty;
    public string TargetEnvironment { get; set; } = "sandbox";
    public string CallbackUrl { get; set; } = string.Empty;
}

public sealed class PaystackOptions : HttpProviderOptions
{
}

public sealed class FlutterwaveOptions : HttpProviderOptions
{
    public string EncryptionKey { get; set; } = string.Empty;
}

public sealed class GhIpssOptions : HttpProviderOptions
{
    public string ParticipantCode { get; set; } = string.Empty;
}

public sealed class TelecelOptions : HttpProviderOptions
{
}

public sealed class AirtelTigoOptions : HttpProviderOptions
{
}

public sealed class ZeepayOptions : HttpProviderOptions
{
}
