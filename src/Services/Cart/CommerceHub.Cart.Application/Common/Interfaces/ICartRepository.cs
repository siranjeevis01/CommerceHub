using CartModel = CommerceHub.Cart.Domain.Models.Cart;

namespace CommerceHub.Cart.Application.Common.Interfaces;

public interface ICartRepository
{
    Task<CartModel?> GetCartAsync(string cartKey, CancellationToken cancellationToken = default);
    Task SaveCartAsync(string cartKey, CartModel cart, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    Task DeleteCartAsync(string cartKey, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string cartKey, CancellationToken cancellationToken = default);
}
