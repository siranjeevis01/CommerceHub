using CommerceHub.Cart.Application.Common.Interfaces;
using MediatR;

namespace CommerceHub.Cart.Application.Commands;

public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand>
{
    private readonly ICartRepository _cartRepository;

    public ClearCartCommandHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        await _cartRepository.DeleteCartAsync(request.CartKey, cancellationToken);
    }
}
