using Microsoft.Extensions.Configuration;
using CommerceHub.Modules.Payment.Application.Common.Interfaces;
using CommerceHub.Modules.Payment.Application.Common.Models;

namespace CommerceHub.Modules.Payment.Infrastructure.Services;

public class UpiQrPaymentGateway : IPaymentGateway
{
    private readonly string _upiId;
    private readonly string _merchantName;

    public UpiQrPaymentGateway(IConfiguration configuration)
    {
        _upiId = configuration["PaymentGateways:Upi:UpiId"]
            ?? Environment.GetEnvironmentVariable("UPI_ID") ?? "";
        _merchantName = configuration["PaymentGateways:Upi:MerchantName"]
            ?? Environment.GetEnvironmentVariable("UPI_MERCHANT_NAME") ?? "CommerceHub";
    }

    public string ProviderName => "UPI";

    public async Task<PaymentResult> CreatePaymentAsync(decimal amount, string currency, Dictionary<string, string> metadata)
    {
        try
        {
            var orderId = metadata.GetValueOrDefault("orderId", Guid.NewGuid().ToString("N"));
            var upiUri = GenerateUpiUri(amount, currency, orderId, metadata.GetValueOrDefault("description", "Payment"));
            var qrBytes = GenerateQrCodePng(upiUri);

            return await Task.FromResult(new PaymentResult
            {
                Success = true,
                TransactionId = $"UPI_{orderId}_{DateTime.UtcNow:yyyyMMddHHmmss}",
                PaymentIntentId = $"upi_{orderId}",
                ClientSecret = Convert.ToBase64String(qrBytes),
                Status = "Pending",
                ErrorMessage = null
            });
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
            Status = "Pending",
            ErrorMessage = null
        });
    }

    public async Task<PaymentResult> RefundPaymentAsync(string paymentIntentId, decimal amount)
    {
        return await Task.FromResult(new PaymentResult
        {
            Success = false,
            ErrorMessage = "UPI payments are non-refundable. Please initiate a manual reversal."
        });
    }

    public async Task<PaymentResult> ConfirmWebhookAsync(string payload, string signature, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(new PaymentResult
        {
            Success = true,
            TransactionId = payload,
            Status = "WebhookReceived"
        });
    }

    public string GenerateUpiUri(decimal amount, string currency, string orderId, string description)
    {
        var encodedName = Uri.EscapeDataString(_merchantName);
        var encodedDesc = Uri.EscapeDataString(description);
        return $"upi://pay?pa={Uri.EscapeDataString(_upiId)}&pn={encodedName}&am={amount:F2}&cu={currency.ToUpper()}&tn={encodedDesc}&tr={Uri.EscapeDataString(orderId)}";
    }

    private static byte[] GenerateQrCodePng(string content)
    {
        try
        {
            var qrCodeType = Type.GetType("QRCoder.QRCodeGenerator, QRCoder");
            if (qrCodeType == null)
                return [];

            var generator = Activator.CreateInstance(qrCodeType)!;
            var getGraphic = qrCodeType.GetMethod("GetGraphic", new[] { typeof(int) })!;
            var pixelData = getGraphic.Invoke(generator, new object[] { 10 })!;
            var getRaw = pixelData.GetType().GetMethod("GetRawImage", Type.EmptyTypes)!;
            var rawBytes = (byte[])getRaw.Invoke(pixelData, null)!;
            return rawBytes;
        }
        catch
        {
            return [];
        }
    }
}
