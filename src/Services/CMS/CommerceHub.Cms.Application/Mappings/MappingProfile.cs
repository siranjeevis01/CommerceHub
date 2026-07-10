using AutoMapper;
using CommerceHub.Cms.Application.DTOs;
using CommerceHub.Cms.Domain.Entities;

namespace CommerceHub.Cms.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CmsPage, PageDto>();
        CreateMap<Banner, BannerDto>();
        CreateMap<Coupon, CouponDto>();
    }
}
