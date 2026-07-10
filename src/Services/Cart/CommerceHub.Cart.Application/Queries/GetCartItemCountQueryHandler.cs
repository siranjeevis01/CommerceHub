using CommerceHub.Cart.Application.Common.Interfaces;
using MediatR;

namespace CommerceHub.Cart.Application.Queries;

public class GetCartItemCountQueryHandler : IRequestHandler<GetCartItemCountQuery, int>
{
    private readonly ICartRepository _cartRepository;

    public GetCartItemCountQueryHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<int> Handle(GetCartItemCountQuery request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetCartAsync(request.CartKey, cancellationToken);

        return cart?.Items.Sum(i => i.Quantity) ?? 0;
    }
}
