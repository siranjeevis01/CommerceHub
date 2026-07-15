using AutoMapper;
using CommerceHub.Modules.Cart.Application.DTOs;
using CartModel = CommerceHub.Modules.Cart.Domain.Models.Cart;
using CartItemModel = CommerceHub.Modules.Cart.Domain.Models.CartItem;

namespace CommerceHub.Modules.Cart.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<(string CartKey, CartModel Cart), CartDto>()
            .ForMember(dest => dest.CartKey, opt => opt.MapFrom(src => src.CartKey))
            .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.Cart.Items.Sum(i => i.Quantity)))
            .ForMember(dest => dest.SubTotal, opt => opt.MapFrom(src => src.Cart.Items.Sum(i => i.TotalPrice)))
            .ForMember(dest => dest.DiscountAmount, opt => opt.MapFrom(src => src.Cart.DiscountAmount))
            .ForMember(dest => dest.CouponCode, opt => opt.MapFrom(src => src.Cart.CouponCode))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src =>
                src.Cart.Items.Sum(i => i.TotalPrice) - (src.Cart.DiscountAmount ?? 0)))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Cart.Items));

        CreateMap<CartItemModel, CartItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Name));
    }
}
