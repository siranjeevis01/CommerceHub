using AutoMapper;
using CommerceHub.Analytics.Domain.Entities;
using CommerceHub.Analytics.Application.DTOs;

namespace CommerceHub.Analytics.Application.Mappings;

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
