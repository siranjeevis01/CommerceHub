using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CommerceHub.Modules.Notification.Application.Common.Interfaces;

namespace CommerceHub.Modules.Notification.Infrastructure.Services;

public class WhatsAppService : IWhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly string _phoneNumberId;
    private readonly string _accessToken;
    private readonly ILogger<WhatsAppService> _logger;

    public WhatsAppService(HttpClient httpClient, IConfiguration configuration, ILogger<WhatsAppService> logger)
    {
        _phoneNumberId = configuration["WhatsApp:PhoneNumberId"] ?? Environment.GetEnvironmentVariable("WHATSAPP_PHONE_NUMBER_ID") ?? "";
        _accessToken = configuration["WhatsApp:AccessToken"] ?? Environment.GetEnvironmentVariable("WHATSAPP_ACCESS_TOKEN") ?? "";
        _httpClient = httpClient;
        _logger = logger;

        if (!string.IsNullOrEmpty(_accessToken))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
    }

    public async Task<bool> SendWhatsAppMessageAsync(string to, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_phoneNumberId) || string.IsNullOrEmpty(_accessToken))
            {
                _logger.LogWarning("WhatsApp not configured. Skipping message to {To}", to);
                return false;
            }

            var payload = new
            {
                messaging_product = "whatsapp",
                recipient_type = "individual",
                to,
                type = "text",
                text = new { body = message }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var url = $"https://graph.facebook.com/v18.0/{_phoneNumberId}/messages";

            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("WhatsApp API error: {StatusCode} {Response}", response.StatusCode, responseBody);
                return false;
            }

            _logger.LogInformation("WhatsApp message sent to {To}", to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send WhatsApp message to {To}", to);
            return false;
        }
    }
}
