using AutoMapper;
using CommerceHub.Modules.Analytics.Domain.Entities;
using CommerceHub.Modules.Analytics.Application.DTOs;

namespace CommerceHub.Modules.Analytics.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AnalyticsEvent, AnalyticsEventDto>();

        CreateMap<DailySummary, DashboardSummaryDto>()
            .ForMember(d => d.NewUsersToday, o => o.MapFrom(s => s.NewUsers))
            .ForMember(d => d.OrdersToday, o => o.MapFrom(s => s.NewOrders))
            .ForMember(d => d.RevenueToday, o => o.MapFrom(s => s.Revenue));
    }
}
