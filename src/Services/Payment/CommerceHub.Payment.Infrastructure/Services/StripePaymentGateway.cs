using Microsoft.Extensions.Configuration;
using Stripe;
using CommerceHub.Payment.Application.Common.Interfaces;
using CommerceHub.Payment.Application.Common.Models;

namespace CommerceHub.Payment.Infrastructure.Services;

public class StripePaymentGateway : IPaymentGateway
{
    private readonly string _secretKey;
    private readonly string _webhookSecret;

    public StripePaymentGateway(IConfiguration configuration)
    {
        _secretKey = configuration["PaymentGateways:Stripe:SecretKey"]
            ?? Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY") ?? "";
        _webhookSecret = configuration["PaymentGateways:Stripe:WebhookSecret"]
            ?? Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET") ?? "";
        StripeConfiguration.ApiKey = _secretKey;
    }

    public string ProviderName => "Stripe";

    public async Task<PaymentResult> CreatePaymentAsync(decimal amount, string currency, Dictionary<string, string> metadata)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100),
                Currency = currency.ToLower(),
                Metadata = new Dictionary<string, string>(metadata)
            };
            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(options);
            return new PaymentResult
            {
                Success = true,
                TransactionId = intent.Id,
                PaymentIntentId = intent.Id,
                ClientSecret = intent.ClientSecret,
                ErrorMessage = null
            };
        }
        catch (StripeException ex)
        {
            return new PaymentResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<PaymentResult> ConfirmPaymentAsync(string paymentIntentId)
    {
        try
        {
            var service = new PaymentIntentService();
            var intent = await service.ConfirmAsync(paymentIntentId);
            return new PaymentResult
            {
                Success = true,
                TransactionId = intent.Id,
                PaymentIntentId = intent.Id,
                ErrorMessage = null
            };
        }
        catch (StripeException ex)
        {
            return new PaymentResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<PaymentResult> RefundPaymentAsync(string paymentIntentId, decimal amount)
    {
        try
        {
            var options = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId,
                Amount = (long)(amount * 100)
            };
            var service = new RefundService();
            var refund = await service.CreateAsync(options);
            return new PaymentResult
            {
                Success = true,
                TransactionId = refund.Id,
                PaymentIntentId = paymentIntentId,
                ErrorMessage = null
            };
        }
        catch (StripeException ex)
        {
            return new PaymentResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<PaymentResult> ConfirmWebhookAsync(string payload, string signature, CancellationToken cancellationToken = default)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(payload, signature, _webhookSecret);
            return new PaymentResult
            {
                Success = true,
                TransactionId = stripeEvent.Id,
                Status = stripeEvent.Type,
                PaymentIntentId = (stripeEvent.Data?.Object as dynamic)?.Id
            };
        }
        catch (StripeException ex)
        {
            return new PaymentResult { Success = false, ErrorMessage = ex.Message };
        }
    }
}
