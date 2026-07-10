using AutoMapper;
using CommerceHub.Payment.Application.DTOs;
using CommerceHub.Payment.Domain.Entities;

namespace CommerceHub.Payment.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Domain.Entities.Payment, PaymentDto>();

        CreateMap<PaymentMethod, PaymentMethodDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Provider));

        CreateMap<Domain.Entities.Payment, PaymentIntentDto>()
            .ForMember(dest => dest.PaymentId, opt => opt.MapFrom(src => src.Id));

        CreateMap<Coupon, CouponDto>();
    }
}
