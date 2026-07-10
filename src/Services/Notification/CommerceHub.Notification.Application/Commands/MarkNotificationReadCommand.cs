using MediatR;
using CommerceHub.Notification.Application.Common.Interfaces;

namespace CommerceHub.Notification.Application.Commands;

public record MarkNotificationReadCommand : IRequest
{
    public int Id { get; init; }
}

public class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand>
{
    private readonly INotificationRepository _notificationRepository;

    public MarkNotificationReadCommandHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        await _notificationRepository.MarkAsReadAsync(request.Id, cancellationToken);
    }
}
