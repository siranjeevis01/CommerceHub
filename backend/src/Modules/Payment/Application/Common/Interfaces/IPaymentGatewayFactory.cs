namespace CommerceHub.Modules.Payment.Application.Common.Interfaces;

public interface IPaymentGatewayFactory
{
    IPaymentGateway GetGateway(string provider);
}
