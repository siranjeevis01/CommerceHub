using AutoMapper;
using CommerceHub.Modules.Cart.Application.Common.Interfaces;
using CommerceHub.Modules.Cart.Application.DTOs;
using CartModel = CommerceHub.Modules.Cart.Domain.Models.Cart;
using CartItemModel = CommerceHub.Modules.Cart.Domain.Models.CartItem;
using MediatR;

namespace CommerceHub.Modules.Cart.Application.Commands;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, CartDto>
{
    private readonly ICartRepository _cartRepository;
    private readonly IMapper _mapper;

    public AddToCartCommandHandler(ICartRepository cartRepository, IMapper mapper)
    {
        _cartRepository = cartRepository;
        _mapper = mapper;
    }

    public async Task<CartDto> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetCartAsync(request.CartKey, cancellationToken)
                   ?? new CartModel { Id = request.CartKey };

        var existingItem = cart.Items.FirstOrDefault(i =>
            i.ProductId == request.ProductId && i.VariantId == request.VariantId);

        if (existingItem is not null)
        {
            existingItem.Quantity += request.Quantity;
        }
        else
        {
            cart.Items.Add(new CartItemModel
            {
                ProductId = request.ProductId,
                VariantId = request.VariantId,
                Name = request.ProductName,
                ImageUrl = request.ImageUrl ?? string.Empty,
                UnitPrice = request.UnitPrice,
                Quantity = request.Quantity
            });
        }

        cart.UpdatedAt = DateTime.UtcNow;

        await _cartRepository.SaveCartAsync(request.CartKey, cart, cancellationToken: cancellationToken);

        return _mapper.Map<CartDto>((request.CartKey, cart));
    }
}
