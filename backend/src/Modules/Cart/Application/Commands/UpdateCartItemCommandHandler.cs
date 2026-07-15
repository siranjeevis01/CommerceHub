using AutoMapper;
using CommerceHub.Modules.Cart.Application.Common.Interfaces;
using CommerceHub.Modules.Cart.Application.DTOs;
using MediatR;

namespace CommerceHub.Modules.Cart.Application.Commands;

public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, CartDto>
{
    private readonly ICartRepository _cartRepository;
    private readonly IMapper _mapper;

    public UpdateCartItemCommandHandler(ICartRepository cartRepository, IMapper mapper)
    {
        _cartRepository = cartRepository;
        _mapper = mapper;
    }

    public async Task<CartDto> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetCartAsync(request.CartKey, cancellationToken)
                   ?? throw new InvalidOperationException($"Cart not found for key: {request.CartKey}");

        var item = cart.Items.FirstOrDefault(i =>
            i.ProductId == request.ProductId && i.VariantId == request.VariantId)
                   ?? throw new InvalidOperationException("Item not found in cart");

        item.Quantity = request.Quantity;
        cart.UpdatedAt = DateTime.UtcNow;

        await _cartRepository.SaveCartAsync(request.CartKey, cart, cancellationToken: cancellationToken);

        return _mapper.Map<CartDto>((request.CartKey, cart));
    }
}
