using Microsoft.Extensions.DependencyInjection;
using CommerceHub.Payment.Application.Common.Interfaces;

namespace CommerceHub.Payment.Infrastructure.Services;

public class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PaymentGatewayFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IPaymentGateway GetGateway(string provider)
    {
        return provider.ToLower() switch
        {
            "stripe" => _serviceProvider.GetRequiredService<StripePaymentGateway>(),
            "razorpay" => _serviceProvider.GetRequiredService<RazorpayPaymentGateway>(),
            _ => _serviceProvider.GetRequiredService<StripePaymentGateway>()
        };
    }
}
