using MediatR;
using AutoMapper;
using CommerceHub.Modules.Notification.Application.DTOs;
using CommerceHub.Modules.Notification.Application.Common.Interfaces;

namespace CommerceHub.Modules.Notification.Application.Queries;

public record GetNotificationByIdQuery : IRequest<NotificationDto?>
{
    public int Id { get; init; }
}

public class GetNotificationByIdQueryHandler : IRequestHandler<GetNotificationByIdQuery, NotificationDto?>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IMapper _mapper;

    public GetNotificationByIdQueryHandler(INotificationRepository notificationRepository, IMapper mapper)
    {
        _notificationRepository = notificationRepository;
        _mapper = mapper;
    }

    public async Task<NotificationDto?> Handle(GetNotificationByIdQuery request, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync(request.Id, cancellationToken);
        return notification is null ? null : _mapper.Map<NotificationDto>(notification);
    }
}
