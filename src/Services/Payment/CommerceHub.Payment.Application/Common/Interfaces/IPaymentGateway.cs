using CommerceHub.Payment.Application.Common.Models;

namespace CommerceHub.Payment.Application.Common.Interfaces;

public interface IPaymentGateway
{
    Task<PaymentResult> CreatePaymentAsync(decimal amount, string currency, Dictionary<string, string> metadata);
    Task<PaymentResult> ConfirmPaymentAsync(string paymentIntentId);
    Task<PaymentResult> RefundPaymentAsync(string paymentIntentId, decimal amount);
    Task<PaymentResult> ConfirmWebhookAsync(string payload, string signature, CancellationToken cancellationToken = default);
    string ProviderName { get; }
}
