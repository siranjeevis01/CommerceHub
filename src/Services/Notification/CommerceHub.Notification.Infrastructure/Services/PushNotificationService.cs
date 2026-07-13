using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CommerceHub.Notification.Application.Common.Interfaces;

namespace CommerceHub.Notification.Infrastructure.Services;

public class PushNotificationService : IPushNotificationService
{
    private readonly ILogger<PushNotificationService> _logger;
    private readonly bool _initialized;

    public PushNotificationService(IConfiguration configuration, ILogger<PushNotificationService> logger)
    {
        _logger = logger;

        try
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                var serviceAccountPath = configuration["Firebase:ServiceAccountPath"]
                    ?? Environment.GetEnvironmentVariable("FIREBASE_SERVICE_ACCOUNT_PATH");
                var serviceAccountJson = configuration["Firebase:ServiceAccountJson"]
                    ?? Environment.GetEnvironmentVariable("FIREBASE_SERVICE_ACCOUNT_JSON");

                if (!string.IsNullOrEmpty(serviceAccountJson))
                {
                    var credential = GoogleCredential.FromJson(serviceAccountJson);
                    FirebaseApp.Create(new AppOptions { Credential = credential });
                    _initialized = true;
                    _logger.LogInformation("Firebase initialized from JSON config");
                }
                else if (!string.IsNullOrEmpty(serviceAccountPath) && File.Exists(serviceAccountPath))
                {
                    var credential = GoogleCredential.FromFile(serviceAccountPath);
                    FirebaseApp.Create(new AppOptions { Credential = credential });
                    _initialized = true;
                    _logger.LogInformation("Firebase initialized from service account file");
                }
                else
                {
                    _logger.LogWarning("Firebase not configured — push notifications will be logged only");
                }
            }
            else
            {
                _initialized = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Firebase");
        }
    }

    public async Task SendPushNotificationAsync(int userId, string title, string message, string? imageUrl = null, string? linkUrl = null, CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            _logger.LogInformation("[Push] Would send to user {UserId}: {Title} - {Message}", userId, title, message);
            return;
        }

        try
        {
            var notification = new Message
            {
                Token = $"user_{userId}",
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = message,
                    ImageUrl = imageUrl
                },
                Data = new Dictionary<string, string>
                {
                    ["userId"] = userId.ToString(),
                    ["linkUrl"] = linkUrl ?? ""
                },
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        ChannelId = "commercehub",
                        ClickAction = "FLUTTER_NOTIFICATION_CLICK"
                    }
                },
                Webpush = new WebpushConfig
                {
                    Notification = new WebpushNotification
                    {
                        Title = title,
                        Body = message,
                        Icon = "/assets/icons/icon-192x192.png",
                        Badge = "/assets/icons/badge-72x72.png"
                    }
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(notification, cancellationToken);
            _logger.LogInformation("Push notification sent to user {UserId}: {MessageId}", userId, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification to user {UserId}", userId);
        }
    }

    public async Task SendBulkPushNotificationAsync(IEnumerable<int> userIds, string title, string message, string? imageUrl = null, string? linkUrl = null, CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            _logger.LogInformation("[Push] Would send bulk to {Count} users: {Title}", userIds.Count(), title);
            return;
        }

        var tokens = userIds.Select(id => $"user_{id}").ToList();

        var messageObj = new MulticastMessage
        {
            Tokens = tokens,
            Notification = new FirebaseAdmin.Messaging.Notification
            {
                Title = title,
                Body = message,
                ImageUrl = imageUrl
            },
            Data = new Dictionary<string, string>
            {
                ["linkUrl"] = linkUrl ?? ""
            }
        };

        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(messageObj, cancellationToken);
            _logger.LogInformation("Bulk push sent: {SuccessCount} success, {FailureCount} failure",
                response.SuccessCount, response.FailureCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send bulk push notification");
        }
    }
}
