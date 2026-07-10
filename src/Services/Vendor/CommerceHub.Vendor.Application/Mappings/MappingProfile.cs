using AutoMapper;
using CommerceHub.Vendor.Application.DTOs;
using CommerceHub.Vendor.Domain.Entities;

namespace CommerceHub.Vendor.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<VendorProfile, VendorDto>()
            .ForMember(d => d.Name, o => o.MapFrom(s => s.StoreName))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.BusinessEmail))
            .ForMember(d => d.Phone, o => o.MapFrom(s => s.BusinessPhone))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.VerificationStatus))
            .ForMember(d => d.IsVerified, o => o.MapFrom(s => s.IsActive));

        CreateMap<VendorProfile, VendorListDto>()
            .ForMember(d => d.Name, o => o.MapFrom(s => s.StoreName))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.VerificationStatus))
            .ForMember(d => d.IsVerified, o => o.MapFrom(s => s.IsActive));

        CreateMap<VendorProfile, StoreDto>();

        CreateMap<VendorPayout, PayoutDto>();

        CreateMap<VendorProfile, VendorAnalyticsDto>()
            .ForMember(d => d.VendorId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.TotalPayouts, o => o.Ignore());
    }
}
