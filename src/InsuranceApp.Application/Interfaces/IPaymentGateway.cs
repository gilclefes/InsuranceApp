namespace InsuranceApp.Application.Interfaces;

public enum PaymentGatewayStatus
{
    Pending,
    Success,
    Failed,
    Unsupported
}

public sealed record PaymentGatewayChargeRequest(
    string ProviderCode,
    string TransactionReference,
    string IdempotencyKey,
    string PolicyNumber,
    decimal Amount,
    string CurrencyCode,
    string CustomerMsisdn,
    string CustomerEmail,
    string Description);

public sealed record PaymentGatewayChargeResponse(
    PaymentGatewayStatus Status,
    string ProviderReference,
    string Message,
    bool RetryRecommended);

public interface IPaymentGateway
{
    string ProviderCode { get; }
    bool CanHandle(string providerCode);
    Task<PaymentGatewayChargeResponse> ChargeAsync(PaymentGatewayChargeRequest request, CancellationToken cancellationToken = default);
}

public interface IPaymentGatewayRouter
{
    IPaymentGateway Resolve(string providerCode);
}

public interface IWebhookSignatureValidator
{
    bool Validate(string providerCode, string payload, string signature);
}
