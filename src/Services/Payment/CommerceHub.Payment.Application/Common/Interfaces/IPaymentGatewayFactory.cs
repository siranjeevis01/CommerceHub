namespace CommerceHub.Payment.Application.Common.Interfaces;

public interface IPaymentGatewayFactory
{
    IPaymentGateway GetGateway(string provider);
}
