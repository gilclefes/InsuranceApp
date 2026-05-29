using InsuranceApp.Application.Interfaces;

namespace InsuranceApp.Infrastructure.Payments.Providers;

public sealed class PaymentGatewayRouter(IEnumerable<IPaymentGateway> gateways, SimulatedPaymentGateway fallback) : IPaymentGatewayRouter
{
    private readonly IReadOnlyList<IPaymentGateway> _gateways = gateways.ToList();

    public IPaymentGateway Resolve(string providerCode)
    {
        var match = _gateways.FirstOrDefault(g => g.CanHandle(providerCode));
        return match ?? fallback;
    }
}
