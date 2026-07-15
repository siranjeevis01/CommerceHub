using Microsoft.Extensions.Configuration;
using Razorpay.Api;
using CommerceHub.Modules.Payment.Application.Common.Interfaces;
using CommerceHub.Modules.Payment.Application.Common.Models;

namespace CommerceHub.Modules.Payment.Infrastructure.Services;

public class RazorpayPaymentGateway : IPaymentGateway
{
    private readonly string _keyId;
    private readonly string _keySecret;

    public RazorpayPaymentGateway(IConfiguration configuration)
    {
        _keyId = configuration["PaymentGateways:Razorpay:KeyId"]
            ?? Environment.GetEnvironmentVariable("RAZORPAY_KEY_ID") ?? "";
        _keySecret = configuration["PaymentGateways:Razorpay:KeySecret"]
            ?? Environment.GetEnvironmentVariable("RAZORPAY_KEY_SECRET") ?? "";
    }

    public string ProviderName => "Razorpay";

    public async Task<PaymentResult> CreatePaymentAsync(decimal amount, string currency, Dictionary<string, string> metadata)
    {
        try
        {
            var client = new RazorpayClient(_keyId, _keySecret);
            var options = new Dictionary<string, object>
            {
                { "amount", amount * 100 },
                { "currency", currency },
                { "notes", metadata }
            };
            var order = await Task.Run(() => client.Order.Create(options));
            return new PaymentResult
            {
                Success = true,
                TransactionId = order["id"].ToString(),
                PaymentIntentId = order["id"].ToString(),
                ErrorMessage = null
            };
        }
        catch (Exception ex)
        {
            return new PaymentResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<PaymentResult> ConfirmPaymentAsync(string paymentIntentId)
    {
        return await Task.FromResult(new PaymentResult
        {
            Success = true,
            TransactionId = paymentIntentId,
            PaymentIntentId = paymentIntentId,
            ErrorMessage = null
        });
    }

    public async Task<PaymentResult> RefundPaymentAsync(string paymentIntentId, decimal amount)
    {
        try
        {
            var client = new RazorpayClient(_keyId, _keySecret);
            var refund = await Task.Run(() => client.Payment.Refund(new Dictionary<string, object>
            {
                { "payment_id", paymentIntentId },
                { "amount", (int)(amount * 100) }
            }));
            return new PaymentResult
            {
                Success = true,
                TransactionId = refund["id"].ToString(),
                PaymentIntentId = paymentIntentId,
                ErrorMessage = null
            };
        }
        catch (Exception ex)
        {
            return new PaymentResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<PaymentResult> ConfirmWebhookAsync(string payload, string signature, CancellationToken cancellationToken = default)
    {
        try
        {
            var expectedSignature = ComputeHmacSha256(payload, _keySecret);
            if (expectedSignature != signature)
            {
                return new PaymentResult { Success = false, ErrorMessage = "Invalid webhook signature" };
            }
            return await Task.FromResult(new PaymentResult
            {
                Success = true,
                TransactionId = "",
                Status = "WebhookReceived"
            });
        }
        catch (Exception ex)
        {
            return new PaymentResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    private static string ComputeHmacSha256(string payload, string secret)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}
