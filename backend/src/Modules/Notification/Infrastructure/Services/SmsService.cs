using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using CommerceHub.Modules.Notification.Application.Common.Interfaces;

namespace CommerceHub.Modules.Notification.Infrastructure.Services;

public class SmsService : ISmsService
{
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _phoneNumber;
    private readonly ILogger<SmsService> _logger;

    public SmsService(IConfiguration configuration, ILogger<SmsService> logger)
    {
        _accountSid = configuration["Twilio:AccountSid"] ?? Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID") ?? "";
        _authToken = configuration["Twilio:AuthToken"] ?? Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN") ?? "";
        _phoneNumber = configuration["Twilio:PhoneNumber"] ?? Environment.GetEnvironmentVariable("TWILIO_PHONE_NUMBER") ?? "";
        _logger = logger;
    }

    public async Task<bool> SendSmsAsync(string to, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            TwilioClient.Init(_accountSid, _authToken);
            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_phoneNumber),
                to: new PhoneNumber(to)
            );
            var success = messageResource.ErrorCode == null;
            if (success)
                _logger.LogInformation("SMS sent to {To}", to);
            else
                _logger.LogError("SMS failed to {To}: {Error}", to, messageResource.ErrorCode);
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {To}", to);
            return false;
        }
    }
}
