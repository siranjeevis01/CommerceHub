using MediatR;
using CommerceHub.Notification.Domain.Models;
using CommerceHub.Notification.Application.Common.Interfaces;

namespace CommerceHub.Notification.Application.Commands;

public record SendPushNotificationCommand : IRequest
{
    public int UserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public string? LinkUrl { get; init; }
    public string? Type { get; init; }
}

public class SendPushNotificationCommandHandler : IRequestHandler<SendPushNotificationCommand>
{
    private readonly IPushNotificationService _pushNotificationService;
    private readonly INotificationHub _notificationHub;
    private readonly INotificationRepository _notificationRepository;

    public SendPushNotificationCommandHandler(
        IPushNotificationService pushNotificationService,
        INotificationHub notificationHub,
        INotificationRepository notificationRepository)
    {
        _pushNotificationService = pushNotificationService;
        _notificationHub = notificationHub;
        _notificationRepository = notificationRepository;
    }

    public async Task Handle(SendPushNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = new NotificationMessage
        {
            UserId = request.UserId,
            Title = request.Title,
            Message = request.Message,
            Type = request.Type ?? "push",
            ImageUrl = request.ImageUrl,
            LinkUrl = request.LinkUrl,
            CreatedAt = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(notification, cancellationToken);

        await _pushNotificationService.SendPushNotificationAsync(
            request.UserId,
            request.Title,
            request.Message,
            request.ImageUrl,
            request.LinkUrl,
            cancellationToken);

        await _notificationHub.SendNotificationToUser(request.UserId, notification);
    }
}
