using AutoMapper;
using CommerceHub.Modules.Cart.Application.Common.Interfaces;
using CommerceHub.Modules.Cart.Application.DTOs;
using MediatR;

namespace CommerceHub.Modules.Cart.Application.Queries;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto?>
{
    private readonly ICartRepository _cartRepository;
    private readonly IMapper _mapper;

    public GetCartQueryHandler(ICartRepository cartRepository, IMapper mapper)
    {
        _cartRepository = cartRepository;
        _mapper = mapper;
    }

    public async Task<CartDto?> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetCartAsync(request.CartKey, cancellationToken);

        return cart is null ? null : _mapper.Map<CartDto>((request.CartKey, cart));
    }
}
