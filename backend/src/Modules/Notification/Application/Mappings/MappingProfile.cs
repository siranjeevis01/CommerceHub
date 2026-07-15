using AutoMapper;
using CommerceHub.Modules.Notification.Domain.Models;
using CommerceHub.Modules.Notification.Application.DTOs;

namespace CommerceHub.Modules.Notification.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<NotificationMessage, NotificationDto>();
    }
}
