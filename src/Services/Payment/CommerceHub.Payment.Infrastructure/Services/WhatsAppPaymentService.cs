using System.Net.Http.Json;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Configuration;

namespace CommerceHub.Payment.Infrastructure.Services;

public class WhatsAppPaymentService
{
    private readonly string _accessToken;
    private readonly string _phoneNumberId;
    private readonly HttpClient _httpClient;

    public WhatsAppPaymentService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _accessToken = configuration["WhatsApp:AccessToken"]
            ?? Environment.GetEnvironmentVariable("WHATSAPP_ACCESS_TOKEN") ?? "";
        _phoneNumberId = configuration["WhatsApp:PhoneNumberId"]
            ?? Environment.GetEnvironmentVariable("WHATSAPP_PHONE_NUMBER_ID") ?? "";
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_accessToken) && !string.IsNullOrWhiteSpace(_phoneNumberId);

    public string GenerateFreeShareLink(string? phoneNumber, string message)
    {
        var encoded = UrlEncoder.Default.Encode(message);
        var phone = phoneNumber?.Replace("+", "").Replace(" ", "").Replace("-", "") ?? "";
        return phone.Length >= 10
            ? $"https://wa.me/{phone}?text={encoded}"
            : $"https://wa.me/?text={encoded}";
    }

    public string GenerateOrderPaymentLink(string? phoneNumber, string orderId, decimal total, string upiId, string merchantName = "CommerceHub")
    {
        var message = $"*Order #{orderId}*\n\n" +
                      $"Total: ₹{total:F2}\n\n" +
                      $"Pay via UPI: {upiId}\n" +
                      $"Open any UPI app (GPay/PhonePe/Paytm) and pay to this UPI ID.\n\n" +
                      $"Thank you for shopping with {merchantName}!";

        return GenerateFreeShareLink(phoneNumber, message);
    }

    public async Task<bool> SendUpiQrCodeAsync(string recipientPhone, string upiUri, decimal amount, string currency, string orderId, CancellationToken ct = default)
    {
        if (!IsConfigured) return false;

        var message = $"*Payment Request - Order #{orderId}*\n\n" +
                      $"Amount: {currency} {amount:F2}\n\n" +
                      $"Pay using UPI: {upiUri}\n\n" +
                      $"Scan the QR code or tap the link to complete payment.";

        var payload = new
        {
            messaging_product = "whatsapp",
            to = recipientPhone,
            type = "text",
            text = new { body = message }
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"https://graph.facebook.com/v18.0/{_phoneNumberId}/messages",
            payload, ct);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SendOrderConfirmationAsync(string recipientPhone, string orderId, decimal amount, CancellationToken ct = default)
    {
        if (!IsConfigured) return false;

        var message = $"*Order Confirmed!*\n\n" +
                      $"Order #{orderId}\n" +
                      $"Amount: ₹{amount:F2}\n\n" +
                      $"Thank you for shopping with CommerceHub!";

        var payload = new
        {
            messaging_product = "whatsapp",
            to = recipientPhone,
            type = "text",
            text = new { body = message }
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"https://graph.facebook.com/v18.0/{_phoneNumberId}/messages",
            payload, ct);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SendShippingUpdateAsync(string recipientPhone, string orderId, string status, CancellationToken ct = default)
    {
        if (!IsConfigured) return false;

        var message = $"*Shipping Update*\n\n" +
                      $"Order #{orderId}\n" +
                      $"Status: {status}\n\n" +
                      $"Track your order in the CommerceHub app.";

        var payload = new
        {
            messaging_product = "whatsapp",
            to = recipientPhone,
            type = "text",
            text = new { body = message }
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"https://graph.facebook.com/v18.0/{_phoneNumberId}/messages",
            payload, ct);

        return response.IsSuccessStatusCode;
    }
}
