using MediatR;
using AutoMapper;
using CommerceHub.Shared.Kernel.Pagination;
using CommerceHub.Modules.Notification.Application.DTOs;
using CommerceHub.Modules.Notification.Application.Common.Interfaces;

namespace CommerceHub.Modules.Notification.Application.Queries;

public record GetUserNotificationsQuery : IRequest<PagedResult<NotificationDto>>
{
    public int UserId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetUserNotificationsQueryHandler : IRequestHandler<GetUserNotificationsQuery, PagedResult<NotificationDto>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IMapper _mapper;

    public GetUserNotificationsQueryHandler(INotificationRepository notificationRepository, IMapper mapper)
    {
        _notificationRepository = notificationRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<NotificationDto>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        var totalCount = await _notificationRepository.GetTotalCountByUserIdAsync(request.UserId, cancellationToken);
        var notifications = await _notificationRepository.GetByUserIdAsync(request.UserId, request.Page, request.PageSize, cancellationToken);
        var items = _mapper.Map<IReadOnlyList<NotificationDto>>(notifications);

        return new PagedResult<NotificationDto>(items, totalCount, request.Page, request.PageSize);
    }
}
