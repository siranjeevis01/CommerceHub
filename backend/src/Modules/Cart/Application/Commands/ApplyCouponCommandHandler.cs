using AutoMapper;
using CommerceHub.Modules.Cart.Application.Common.Interfaces;
using CommerceHub.Modules.Cart.Application.DTOs;
using CommerceHub.Shared.Contracts.Events;
using MassTransit;
using MediatR;

namespace CommerceHub.Modules.Cart.Application.Commands;

public class ApplyCouponCommandHandler : IRequestHandler<ApplyCouponCommand, CartDto>
{
    private readonly ICartRepository _cartRepository;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public ApplyCouponCommandHandler(ICartRepository cartRepository, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _cartRepository = cartRepository;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<CartDto> Handle(ApplyCouponCommand request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetCartAsync(request.CartKey, cancellationToken)
                   ?? throw new InvalidOperationException($"Cart not found for key: {request.CartKey}");

        cart.CouponCode = request.CouponCode.ToUpperInvariant();
        cart.DiscountAmount = cart.Items.Sum(i => i.TotalPrice) * 0.10m;
        cart.UpdatedAt = DateTime.UtcNow;

        await _cartRepository.SaveCartAsync(request.CartKey, cart, cancellationToken: cancellationToken);

        if (cart.UserId.HasValue)
        {
            await _publishEndpoint.Publish(new CouponApplied
            {
                Code = cart.CouponCode ?? request.CouponCode,
                UserId = cart.UserId.Value,
                DiscountAmount = cart.DiscountAmount ?? 0
            }, cancellationToken);
        }

        return _mapper.Map<CartDto>((request.CartKey, cart));
    }
}
