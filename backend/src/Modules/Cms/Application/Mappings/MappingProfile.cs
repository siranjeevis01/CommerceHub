using AutoMapper;
using CommerceHub.Modules.Cms.Application.DTOs;
using CommerceHub.Modules.Cms.Domain.Entities;

namespace CommerceHub.Modules.Cms.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CmsPage, PageDto>();
        CreateMap<Banner, BannerDto>();
        CreateMap<Coupon, CouponDto>();
    }
}
