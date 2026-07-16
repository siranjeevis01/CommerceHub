using MassTransit;
using CommerceHub.Modules.Notification.Application.Common.Interfaces;
using CommerceHub.Shared.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace CommerceHub.Modules.Notification.Presentation.Consumers;

public class UserRegisteredConsumer : IConsumer<UserRegistered>
{
    private readonly IUserLookupService _userLookup;
    private readonly IEmailService _emailService;
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(IUserLookupService userLookup, IEmailService emailService, ILogger<UserRegisteredConsumer> logger)
    {
        _userLookup = userLookup;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegistered> context)
    {
        var msg = context.Message;
        _logger.LogInformation("User registered: {UserId} ({Email})", msg.UserId, msg.Email);

        await _userLookup.CacheUserAsync(msg.UserId, msg.Email, msg.FirstName, msg.LastName, context.CancellationToken);

        await _emailService.SendEmailWithTemplateAsync(msg.Email, "welcome", new Dictionary<string, string>
        {
            ["CustomerName"] = $"{msg.FirstName} {msg.LastName}".Trim()
        }, context.CancellationToken);
    }
}
