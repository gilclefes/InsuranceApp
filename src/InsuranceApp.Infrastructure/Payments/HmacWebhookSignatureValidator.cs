using System.Security.Cryptography;
using System.Text;
using InsuranceApp.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace InsuranceApp.Infrastructure.Payments;

public sealed class HmacWebhookSignatureValidator(IOptions<PaymentGatewayOptions> options) : IWebhookSignatureValidator
{
    public bool Validate(string providerCode, string payload, string signature)
    {
        if (string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        var secret = ResolveSecret(providerCode);
        if (string.IsNullOrWhiteSpace(secret))
        {
            // No secret configured (e.g. dev/test) — accept any non-empty signature.
            return true;
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            return false;
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var expected = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(signature.Trim().Replace("sha256=", string.Empty, StringComparison.OrdinalIgnoreCase)));
    }

    private string ResolveSecret(string providerCode)
    {
        var cfg = options.Value;
        return providerCode?.ToUpperInvariant() switch
        {
            "HUBTEL" or "HUBTEL_MOMO" => cfg.Hubtel.WebhookSecret,
            "PAYSTACK" => cfg.Paystack.WebhookSecret,
            "FLUTTERWAVE" => cfg.Flutterwave.WebhookSecret,
            "MTN_MOMO" => cfg.MtnMomo.WebhookSecret,
            "GHIPSS_ACH" or "GHIPSS_GIP" => cfg.GhIpss.WebhookSecret,
            "TELECEL_CASH" => cfg.Telecel.WebhookSecret,
            "AT_MONEY" => cfg.AirtelTigo.WebhookSecret,
            "ZEEPAY" => cfg.Zeepay.WebhookSecret,
            _ => string.Empty
        };
    }
}
