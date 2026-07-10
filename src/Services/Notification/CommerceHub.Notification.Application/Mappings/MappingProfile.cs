using AutoMapper;
using CommerceHub.Notification.Domain.Models;
using CommerceHub.Notification.Application.DTOs;

namespace CommerceHub.Notification.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<NotificationMessage, NotificationDto>();
    }
}
