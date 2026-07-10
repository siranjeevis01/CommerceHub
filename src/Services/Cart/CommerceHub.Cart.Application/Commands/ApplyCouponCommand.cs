using MediatR;
using CommerceHub.Cart.Application.DTOs;

namespace CommerceHub.Cart.Application.Commands;

public record ApplyCouponCommand : IRequest<CartDto>
{
    public string CartKey { get; init; } = string.Empty;
    public string CouponCode { get; init; } = string.Empty;
}
