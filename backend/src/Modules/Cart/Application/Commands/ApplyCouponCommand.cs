using MediatR;
using CommerceHub.Modules.Cart.Application.DTOs;

namespace CommerceHub.Modules.Cart.Application.Commands;

public record ApplyCouponCommand : IRequest<CartDto>
{
    public string CartKey { get; init; } = string.Empty;
    public string CouponCode { get; init; } = string.Empty;
}
